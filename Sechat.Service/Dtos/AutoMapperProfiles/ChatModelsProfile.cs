using AutoMapper;
using Sechat.Data.Models.ChatModels;
using Sechat.Data.Models.UserDetails;
using Sechat.Service.Dtos.ChatDtos;

namespace Sechat.Service.Dtos.AutoMapperProfiles;

public class ChatModelsProfile : Profile
{

    public ChatModelsProfile()
    {
        _ = CreateMap<Room, RoomDto>();
        _ = CreateMap<Message, RoomMessageDto>();
        _ = CreateMap<UserProfile, RoomMemberDto>();
        _ = CreateMap<MessageViewer, MessageViewerDto>().ForMember(x => x.User, y => y.MapFrom(z => z.UserId));
    }
}
