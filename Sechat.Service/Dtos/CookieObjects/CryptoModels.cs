namespace Sechat.Service.Dtos.CookieObjects;

public class MessageDecryptionRequest
{
    public long Id { get; set; }
    public string Message { get; set; }
    public string RoomId { get; set; }
}