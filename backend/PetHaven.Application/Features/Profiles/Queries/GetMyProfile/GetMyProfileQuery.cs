using MediatR;
using PetHaven.Application.DTOs.Profile;

namespace PetHaven.Application.Features.Profiles.Queries.GetMyProfile;

public class GetMyProfileQuery : IRequest<MyProfileDto>
{
    public Guid UserId { get; set; }
}