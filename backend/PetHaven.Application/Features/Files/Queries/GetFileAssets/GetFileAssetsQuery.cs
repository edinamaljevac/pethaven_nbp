using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Files.Queries.GetFileAssets;

public class GetFileAssetsQuery : IRequest<List<FileAssetDto>>
{
    public Guid? UploadedByUserId { get; set; }
}
