using System;

namespace Sechat.Service.Dtos.ChatDtos;

public class CallLogDto
{
    public long Id { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string CalleeName { get; set; }
    public string PhonerName { get; set; }

    public bool WasViewed { get; set; }

    public string VideoCallType { get; set; }
    public string VideoCallResult { get; set; }
    public string UserProfileId { get; set; }
}
