using Sechat.Algo;

namespace Sechat.Service.Services.CacheServices;

public class ContactSuggestionsCache
{
    public Graph<ContactSuggestion> Cache { get; set; } = new();
}
