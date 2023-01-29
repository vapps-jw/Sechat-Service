using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<IdentityUser> _userManager;
    private readonly UserRepository _userRepository;
    private readonly ChatRepository _chatRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public ChatController(
          UserManager<IdentityUser> userManager,
    UserRepository userRepository,
        ChatRepository chatRepository,
        IMapper mapper,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _mapper = mapper;
        _chatHubContext = chatHubContext;
    }

    [HttpGet("get-state")]
    public async Task<IActionResult> GetState()
    {
        var rooms = await _chatRepository.GetRooms(UserId);
        var connections = await _userRepository.GetConnections(UserId);

        var roomDtos = _mapper.Map<List<RoomDto>>(rooms);
        var connectionDtos = _mapper.Map<List<UserConnectionDto>>(connections);

        var res = new StateDto
        {
            Rooms = roomDtos,
            UserConnections = connectionDtos
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

    [HttpPost("add-to-room")]
    public async Task<IActionResult> AddToRoom([FromBody] RoomMemberUpdateRequest roomMemberUpdate)
    {
        var user = await _userManager.FindByNameAsync(roomMemberUpdate.UserName);
        var connection = _userRepository.GetConnection(UserId, user.Id);
        if (connection is null || connection.Blocked) return BadRequest();

        var room = _chatRepository.AddToRoom(roomMemberUpdate.RoomId, user.Id);
        if (await _chatRepository.SaveChanges() > 0)
        {
            var roomDto = _mapper.Map<RoomDto>(room);
            await _chatHubContext.Clients.Group(room.Id).RoomUpdated(roomDto);
            return Ok();
        }

        return BadRequest();
    }

    [HttpPost("remove-from-room")]
    public IActionResult RemoveFromRoom([FromBody] RoomMemberUpdateRequest roomMemberUpdate) => BadRequest();

    [HttpPost("leave-room")]
    public IActionResult LeaveRoom([FromBody] RoomMemberUpdateRequest roomMemberUpdate) => BadRequest();

    [HttpDelete("delete-room")]
    public async Task<IActionResult> DeleteRoom(string roomId)
    {
        _chatRepository.DeleteRoom(roomId, UserId);

        if (await _chatRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(roomId).RoomDeleted(new ResourceGuid(roomId));
        }

        return Ok();
    }
}
