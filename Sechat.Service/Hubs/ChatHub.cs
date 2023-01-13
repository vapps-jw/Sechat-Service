using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Dtos.SignalRDtos;
using Sechat.Service.Services;
using System;
using System.Threading.Tasks;

namespace Sechat.Service.Hubs;

public interface IChatHub
{

}

[Authorize]
public class ChatHub : SechatHubBase<IChatHub>
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IMapper _mapper;
    private readonly IEncryptor _encryptor;
    private readonly ChatRepository _chatRepository;

    public ChatHub(
        ILogger<ChatHub> logger,
        IMapper mapper,
        IEncryptor encryptor,
        ChatRepository chatRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _encryptor = encryptor;
        _chatRepository = chatRepository;
    }

    public void LogConnection(ConnectionEstablishedDto connectionEstablishedDto) =>
        _logger.LogWarning("Connection established for user Id: {0} Name: {1} Message: {2}", UserId, UserName, connectionEstablishedDto.Message);

    public async Task<RoomDto> CreateRoom(CreateRoomRequest request)
    {
        try
        {
            var newRoom = _chatRepository.CreateRoom(request.RoomName, UserId, _encryptor.GenerateKey());
            if (await _chatRepository.SaveChanges() == 0)
            {
                throw new Exception("Room creation failed");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, newRoom.Id.ToString());
            var responseDto = _mapper.Map<RoomDto>(newRoom);

            return responseDto;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }
}
