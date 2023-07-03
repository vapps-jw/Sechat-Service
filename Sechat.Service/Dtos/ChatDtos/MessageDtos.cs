using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.ChatDtos;

public class MessageDto
{
    public long Id { get; set; }
    public string NameSentBy { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public string RoomId { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public bool Error { get; set; }

    public bool WasViewed { get; set; }

    public List<MessageViewerDto> MessageViewers { get; set; } = new();
}

public class DirectMessageDto
{
    public long Id { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string IdSentBy { get; set; } = string.Empty;
    public string NameSentBy { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
    public bool WasViewed { get; set; }

    public long ContactId { get; set; }
}

public class MessageViewerDto
{
    public string User { get; set; }
}

public record MessageToDecrypt
{
    public string Id { get; set; }
    public string Text { get; set; }
    public string RoomId { get; set; }
}
