namespace Sechat.Service.Dtos.CookieObjects;

public class MessageDecryptionRequest
{
    public long Id { get; set; }
    public string Message { get; set; }
    public string RoomId { get; set; }
    public bool Error { get; set; }
}

public class DirectMessageDecryptionRequest
{
    public long Id { get; set; }
    public string Message { get; set; }
    public long ContactId { get; set; }
    public bool Error { get; set; }
}