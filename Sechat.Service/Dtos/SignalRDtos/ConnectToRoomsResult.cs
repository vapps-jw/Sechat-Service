using System.Collections.Generic;

namespace Sechat.Service.Dtos.SignalRDtos;

public class ConnectToRoomsResult
{
    public List<string> ConnectedRooms { get; set; } = new();
}
