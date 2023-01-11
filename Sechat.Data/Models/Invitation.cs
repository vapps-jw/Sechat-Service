namespace Sechat.Data.Models;
public record Invitation : BaseModel<long>
{
    public bool Approved { get; set; }
    public string Inviter { get; set; }
    public string Invited { get; set; }
}
