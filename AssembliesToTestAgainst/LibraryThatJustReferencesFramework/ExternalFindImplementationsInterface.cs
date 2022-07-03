using System;

namespace LibraryThatJustReferencesFramework
{
    public interface ExternalFindImplementationsInterface
    {
        void BasicMethod();
        string BasicProperty { get; set; }
        event EventHandler BasicEvent;
    }
}