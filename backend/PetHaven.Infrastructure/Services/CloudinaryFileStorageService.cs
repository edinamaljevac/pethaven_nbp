using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using PetHaven.Application.Interfaces;
using PetHaven.Application.Settings;
using PetHaven.Domain.Entities;

namespace PetHaven.Infrastructure.Services;

/// <summary>
/// Stores uploaded files (images and videos) in Cloudinary. Required for free hosting
/// providers whose local filesystem is ephemeral (Render, Azure F1, Fly, etc.), where
/// anything written to wwwroot/uploads is lost on every restart or redeploy.
/// </summary>
public class CloudinaryFileStorageService : IFileStorageService
{
    private const string UploadFolder = "pethaven";
    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<FileAsset> SaveAsync(Stream stream, string originalFileName, string contentType, long sizeBytes, Guid? uploadedByUserId, CancellationToken cancellationToken)
    {
        var fileDescription = new FileDescription(originalFileName, stream);
        RawUploadResult result;

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            result = await _cloudinary.UploadAsync(
                new ImageUploadParams { File = fileDescription, Folder = UploadFolder },
                cancellationToken);
        }
        else if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            result = await _cloudinary.UploadAsync(
                new VideoUploadParams { File = fileDescription, Folder = UploadFolder },
                cancellationToken);
        }
        else
        {
            result = await _cloudinary.UploadAsync(
                new RawUploadParams { File = fileDescription, Folder = UploadFolder },
                "auto",
                cancellationToken);
        }

        if (result.Error is not null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        return new FileAsset
        {
            OriginalFileName = originalFileName,
            StoredFileName = result.PublicId,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Url = result.SecureUrl.ToString(),
            UploadedByUserId = uploadedByUserId
        };
    }
}
