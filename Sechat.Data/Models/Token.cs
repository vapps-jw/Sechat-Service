namespace Sechat.Data.Models;
public record Token : BaseModel<long>
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
