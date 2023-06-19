using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.Models.VideoCalls;
public record CallLog : BaseModel<long>
{
    public string CalleeId { get; set; }
    public VideoCallType VideoCallType { get; set; }
    public VideoCallResult VideoCallResult { get; set; }

    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
