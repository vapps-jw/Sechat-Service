using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.Models.NotebookModels;
public record Notebook : BaseModel<string>
{
    public string Name { get; set; }
    public bool Public { get; set; }

    public List<Note> Notes { get; set; }

    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
