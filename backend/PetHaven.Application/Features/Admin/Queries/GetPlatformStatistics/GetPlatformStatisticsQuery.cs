using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Admin.Queries.GetPlatformStatistics;

public class GetPlatformStatisticsQuery : IRequest<PlatformStatisticsDto>;
