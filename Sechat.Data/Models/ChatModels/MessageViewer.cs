using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models.ChatModels;
public record MessageViewer
{
    public long Id { get; set; }

    [Required]
    [MaxLength(36)]
    public string UserId { get; set; }

    public Message Message { get; set; }
    public long MessageId { get; set; }

    public MessageViewer(string userId) => UserId = userId;
}
