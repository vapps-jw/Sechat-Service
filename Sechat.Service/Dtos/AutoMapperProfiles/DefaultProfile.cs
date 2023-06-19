using AutoMapper;
using Sechat.Data.Models.UserDetails;
using Sechat.Service.Dtos.ChatDtos;

namespace Sechat.Service.Dtos.AutoMapperProfiles;

public class DefaultProfile : Profile
{
    public DefaultProfile()
    {
        _ = CreateMap<UserProfile, UserProfileProjection>();
        _ = CreateMap<UserProfileProjection, UserProfile>();
        _ = CreateMap<UserConnection, UserContactDto>();
        _ = CreateMap<PushSubscriptionDto, NotificationSubscription>()
            .ForMember(x => x.Auth, y => y.MapFrom(z => z.Keys.Auth))
            .ForMember(x => x.P256dh, y => y.MapFrom(z => z.Keys.P256dh));
    }
}
