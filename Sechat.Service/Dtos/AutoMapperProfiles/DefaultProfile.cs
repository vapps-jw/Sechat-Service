using AutoMapper;
using Sechat.Data.Models;
using Sechat.Service.Dtos.ChatDtos;

namespace Sechat.Service.Dtos.AutoMapperProfiles;

public class DefaultProfile : Profile
{
    public DefaultProfile()
    {
        _ = CreateMap<UserProfile, UserProfileProjection>();
        _ = CreateMap<UserProfileProjection, UserProfile>();
        _ = CreateMap<UserConnection, UserConnectionDto>();
    }
}
