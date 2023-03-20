using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models;

public record UserProfile : BaseTrackedModel<string>
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    public List<Feature> Features { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Key> Keys { get; set; } = new();
    public List<NotificationSubscription> NotificationSubscriptions { get; set; } = new();
}
