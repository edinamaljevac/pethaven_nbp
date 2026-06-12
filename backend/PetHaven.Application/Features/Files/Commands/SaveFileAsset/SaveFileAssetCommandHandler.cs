using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Files.Commands.SaveFileAsset;

public class SaveFileAssetCommandHandler : IRequestHandler<SaveFileAssetCommand, FileAssetDto>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public SaveFileAssetCommandHandler(IFileStorageService fileStorageService, IUnitOfWork uow, IMapper mapper)
    {
        _fileStorageService = fileStorageService;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<FileAssetDto> Handle(SaveFileAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _fileStorageService.SaveAsync(request.Stream, request.OriginalFileName, request.ContentType, request.SizeBytes, request.UploadedByUserId, cancellationToken);
        await _uow.FileAssets.AddAsync(asset);
        await _uow.SaveChangesAsync();
        return _mapper.Map<FileAssetDto>(asset);
    }
}