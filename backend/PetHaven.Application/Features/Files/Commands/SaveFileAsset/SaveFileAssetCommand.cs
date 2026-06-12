using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Files.Commands.SaveFileAsset;

public class SaveFileAssetCommand : IRequest<FileAssetDto>
{
    public Stream Stream { get; set; } = Stream.Null;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public Guid? UploadedByUserId { get; set; }
}