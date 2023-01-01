namespace Sechat.Data.Models
{
    public abstract record BaseModel<TKey>
    {
        public TKey? Id { get; set; }
    }
}
