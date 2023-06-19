namespace Sechat.Data.Models.Abstractions;

public abstract record BaseModel<TKey>
{
    public TKey Id { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
