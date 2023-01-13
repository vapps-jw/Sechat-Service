namespace Sechat.Service.Dtos.SignalRDtos;

public class IncomingMessageDto
{
    public string SenderId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
}
