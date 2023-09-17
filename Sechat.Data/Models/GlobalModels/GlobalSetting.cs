using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.GlobalModels;
public record GlobalSetting : BaseModel<string>
{
    public string Value { get; set; } = string.Empty;
}
