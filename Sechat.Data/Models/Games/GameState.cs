using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.Games;

public record GameState : BaseModel<long>
{
    public string GameName { get; set; }
    public string State { get; set; }
    public List<string> Players { get; set; } = new();
}
