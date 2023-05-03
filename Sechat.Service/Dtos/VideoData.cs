using System.Text.Json.Serialization;

namespace Sechat.Service.Dtos;

public readonly struct VideoData
{
    public int Index { get; }
    public string Part { get; }
    public string UserName { get; }

    [JsonConstructor]
    public VideoData(int index, string part, string userName) => (Index, Part, UserName) = (index, part, userName);
}
