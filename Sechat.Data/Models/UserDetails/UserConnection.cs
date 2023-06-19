using Sechat.Data.Models.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models.UserDetails;
public record UserConnection : BaseModel<long>
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
}
