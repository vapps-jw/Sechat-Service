using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Dtos.SignalRDtos;
using Sechat.Service.Hubs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class ChatController : SechatControllerBase
{
    private readonly ChatRepository _chatRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public ChatController(ChatRepository chatRepository, IMapper mapper, IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _chatRepository = chatRepository;
        _mapper = mapper;
        _chatHubContext = chatHubContext;
    }

    [HttpGet("get-state")]
    public async Task<IActionResult> GetState()
    {
        var rooms = await _chatRepository.GetRooms(UserId);
        var responseDtos = _mapper.Map<List<RoomDto>>(rooms);

        var res = new StateDto
        {
            Rooms = responseDtos
        };

        return Ok(res);
    }

    [HttpGet("get-my-rooms")]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _chatRepository.GetRooms(UserId);
        var responseDtos = _mapper.Map<List<RoomDto>>(rooms);

        return Ok(responseDtos);
    }

    [HttpDelete("delete-room")]
    public async Task<IActionResult> DeleteRoom(string roomId)
    {
        _chatRepository.DeleteRoom(roomId, UserId);

        if (await _chatRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(roomId).RoomDeleted(new RoomIdMessage(roomId));
        }

        return Ok();
    }
}
