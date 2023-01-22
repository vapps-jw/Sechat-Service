using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models;
public record UserConnection : BaseModel<long>
{
    public bool Approved { get; set; }
    [Required]
    public string InviterId { get; set; }
    [Required]
    public string InviterName { get; set; }
    [Required]
    public string InvitedId { get; set; }
    [Required]
    public string InvitedName { get; set; }
}
