using AutoMapper;
using Sechat.Data.Models.ChatModels;
using Sechat.Data.Models.UserDetails;
using Sechat.Data.Models.VideoCalls;
using Sechat.Service.Dtos.ChatDtos;

namespace Sechat.Service.Dtos.AutoMapperProfiles;

public class ChatModelsProfile : Profile
{

    public ChatModelsProfile()
    {
        _ = CreateMap<Room, RoomDto>();
        _ = CreateMap<Message, MessageDto>();
        _ = CreateMap<Contact, ContactDto>();
        _ = CreateMap<DirectMessage, DirectMessageDto>();
        _ = CreateMap<UserProfile, RoomMemberDto>();
        _ = CreateMap<Blacklisted, BlacklistedDto>();
        _ = CreateMap<CallLog, CallLogDto>()
            .ForMember(x => x.VideoCallType, y => y.MapFrom(z => z.VideoCallType))
            .ForMember(x => x.VideoCallResult, y => y.MapFrom(z => z.VideoCallResult));
        _ = CreateMap<MessageViewer, MessageViewerDto>().ForMember(x => x.User, y => y.MapFrom(z => z.UserId));
    }
}
