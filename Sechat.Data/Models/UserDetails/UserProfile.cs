using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.ChatModels;
using Sechat.Data.Models.VideoCalls;
using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models.UserDetails;

public record UserProfile : BaseTrackedModel<string>
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    public bool PrivacyPolicyAccepted { get; set; }
    public bool ToSAccepted { get; set; }

    public List<Feature> Features { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Key> Keys { get; set; } = new();
    public List<NotificationSubscription> NotificationSubscriptions { get; set; } = new();
    public List<CallLog> CallLogs { get; set; } = new();
}
