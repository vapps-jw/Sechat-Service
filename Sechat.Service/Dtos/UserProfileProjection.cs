using System;

namespace Sechat.Service.Dtos;

public class UserProfileProjection
{
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
