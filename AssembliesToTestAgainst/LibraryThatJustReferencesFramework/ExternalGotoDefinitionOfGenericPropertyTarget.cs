using System.Collections.Generic;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionOfGenericPropertyTarget<TValue>
    {
        public IReadOnlyList<TValue> Values { get; set; }
    }
}