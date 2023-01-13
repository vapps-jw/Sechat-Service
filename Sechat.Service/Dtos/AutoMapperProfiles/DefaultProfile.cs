using AutoMapper;
using Sechat.Data.Models;

namespace Sechat.Service.Dtos.AutoMapperProfiles;

public class DefaultProfile : Profile
{
    public DefaultProfile()
    {
        _ = CreateMap<UserProfile, UserProfileProjection>();
        _ = CreateMap<UserProfileProjection, UserProfile>();

    }
}
