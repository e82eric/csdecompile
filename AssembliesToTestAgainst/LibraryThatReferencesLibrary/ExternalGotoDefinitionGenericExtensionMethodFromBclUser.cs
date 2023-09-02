using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionGenericExtensionMethodFromBclUser
    {
        public void Run(ConcurrentDictionary<string, string> a, ICollection<string> b)
        {
            a.Values.ToArray();
            b.ToArray();
        }
    }
}