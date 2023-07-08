using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.NotebookModels;
public record Note : BaseModel<long>
{
    public string Name { get; set; }
    public bool Done { get; set; }

    public string NotebookId { get; set; } = string.Empty;
    public Notebook Notebook { get; set; }
}
