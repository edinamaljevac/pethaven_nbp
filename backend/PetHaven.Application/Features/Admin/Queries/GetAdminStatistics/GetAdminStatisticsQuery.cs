using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Admin.Queries.GetAdminStatistics;
public class GetAdminStatisticsQuery : IRequest<AdminStatisticsDto> { }