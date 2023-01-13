namespace Sechat.Data.Models;
public abstract record BaseTrackedModel<TKey> : BaseModel<TKey>
{
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}
