using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    public async Task<FileAsset> SaveAsync(Stream stream, string originalFileName, string contentType, long sizeBytes, Guid? uploadedByUserId, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(root);
        var fullPath = Path.Combine(root, storedFileName);

        await using var output = File.Create(fullPath);
        await stream.CopyToAsync(output, cancellationToken);

        return new FileAsset
        {
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Url = $"/uploads/{storedFileName}",
            UploadedByUserId = uploadedByUserId
        };
    }
}