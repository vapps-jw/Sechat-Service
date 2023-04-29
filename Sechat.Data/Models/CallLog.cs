namespace Sechat.Data.Models;
public record CallLog : BaseModel<long>
{
    public string CallerId { get; set; }
    public VideoCallType VideoCallType { get; set; }
    public VideoCallResult VideoCallResult { get; set; }

    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
