namespace Sechat.Data.Models;
public record UserConnection : BaseModel<long>
{
    public bool Approved { get; set; }
    public string Inviter { get; set; }
    public string Invited { get; set; }
}
