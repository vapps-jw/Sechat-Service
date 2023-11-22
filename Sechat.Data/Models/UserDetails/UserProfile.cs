using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.CalendarModels;
using Sechat.Data.Models.ChatModels;
using Sechat.Data.Models.VideoCalls;
using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models.UserDetails;

public record UserProfile : BaseTrackedModel<string>
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    public bool PrivacyPolicyAccepted { get; set; } = true;
    public bool ToSAccepted { get; set; } = true;
    public bool InvitationsAllowed { get; set; } = true;
    public string ProfilePicture { get; set; } = string.Empty;
    public string ReferralPass { get; set; } = string.Empty;

    public List<Feature> Features { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Key> Keys { get; set; } = new();
    public List<NotificationSubscription> NotificationSubscriptions { get; set; } = new();
    public List<CallLog> CallLogs { get; set; } = new();
    public List<Blacklisted> Blacklist { get; set; } = new();
    public Calendar Calendar { get; set; }

    public override string ToString() => UserName;
}
