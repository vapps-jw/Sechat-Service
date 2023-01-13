using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos.ChatDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class ChatController : SechatControllerBase
{
    private readonly ChatRepository _chatRepository;
    private readonly IMapper _mapper;

    public ChatController(ChatRepository chatRepository, IMapper mapper)
    {
        _chatRepository = chatRepository;
        _mapper = mapper;
    }

    [HttpGet("get-my-rooms")]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _chatRepository.GetRooms(UserId);
        var responseDtos = _mapper.Map<List<RoomDto>>(rooms);

        return Ok(responseDtos);
    }
}
