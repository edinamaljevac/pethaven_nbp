using PetHaven.Domain.Entities;

namespace PetHaven.Application.Interfaces;

public interface IFileStorageService
{
    Task<FileAsset> SaveAsync(Stream stream, string originalFileName, string contentType, long sizeBytes, Guid? uploadedByUserId, CancellationToken cancellationToken);
}