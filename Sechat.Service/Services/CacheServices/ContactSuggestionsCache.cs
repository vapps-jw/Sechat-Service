using Sechat.Algo;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sechat.Service.Services.CacheServices;

public class ContactSuggestionsCache
{
    public Graph<ContactSuggestion> Cache { get; set; } = new();

    public class EqualityComparer : IEqualityComparer<ContactSuggestion>
    {
        public bool Equals(ContactSuggestion x, ContactSuggestion y) => x.Equals(y);
        public int GetHashCode([DisallowNull] ContactSuggestion obj) => (int)obj.UserName.ToCharArray().Select(char.GetNumericValue).Aggregate((a, i) => a + i);
    }
}
