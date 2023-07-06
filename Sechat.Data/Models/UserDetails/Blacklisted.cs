using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.UserDetails;
public record Blacklisted : BaseModel<long>
{
    public string Name { get; set; }
}
