using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Files.Queries.GetFileAssets;

public class GetFileAssetsQueryHandler : IRequestHandler<GetFileAssetsQuery, List<FileAssetDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFileAssetsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<FileAssetDto>> Handle(GetFileAssetsQuery request, CancellationToken cancellationToken)
    {
        var files = await _unitOfWork.FileAssets.GetAllAsync();

        if (request.UploadedByUserId.HasValue)
        {
            files = files.Where(x => x.UploadedByUserId == request.UploadedByUserId.Value).ToList();
        }

        return _mapper.Map<List<FileAssetDto>>(files.OrderByDescending(x => x.CreatedAt).ToList());
    }
}
