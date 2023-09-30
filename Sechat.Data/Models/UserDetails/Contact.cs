using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.ChatModels;
using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models.UserDetails;
public record Contact : BaseModel<long>
{
    public bool Approved { get; set; }
    [Required]
    public string InviterId { get; set; } = string.Empty;
    [Required]
    public string InviterName { get; set; } = string.Empty;
    [Required]
    public string InvitedId { get; set; } = string.Empty;
    [Required]
    public string InvitedName { get; set; } = string.Empty;

    public bool Blocked { get; set; }
    public string BlockedById { get; set; } = string.Empty;
    public string BlockedByName { get; set; } = string.Empty;

    public List<DirectMessage> DirectMessages { get; set; } = new();

    public override string ToString() => $"{InviterName} -> {InvitedName}";
}
