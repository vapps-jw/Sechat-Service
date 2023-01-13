using AutoMapper;
using Sechat.Data.Models;
using Sechat.Service.Dtos.ChatDtos;

namespace Sechat.Service.Dtos.AutoMapperProfiles;

public class ChatModelsProfile : Profile
{

    public ChatModelsProfile()
    {
        _ = CreateMap<Room, RoomDto>();
        _ = CreateMap<Message, RoomMessageDto>();
        _ = CreateMap<UserProfile, RoomMemberDto>();
    }
}
