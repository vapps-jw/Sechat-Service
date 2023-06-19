namespace Sechat.Data.Models.Abstractions;
public abstract record BaseTrackedModel<TKey> : BaseModel<TKey>
{
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}
