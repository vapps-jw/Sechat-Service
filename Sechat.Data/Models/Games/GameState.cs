using Sechat.Data.Models.Abstractions;
using System.Text.Json;

namespace Sechat.Data.Models.Games;

public record GameState : BaseModel<long>
{
    public string GameName { get; set; }
    public JsonDocument State { get; set; }
    public List<string> Players { get; set; } = new();
}
