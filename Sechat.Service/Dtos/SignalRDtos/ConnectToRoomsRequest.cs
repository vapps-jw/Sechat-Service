using System.Collections.Generic;

namespace Sechat.Service.Dtos.SignalRDtos;

public class ConnectToRoomsRequest
{
    public List<string> RoomIds { get; set; } = new();
}
