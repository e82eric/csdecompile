using System;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindImplementationsInterfaceInheritor : ExternalFindImplementationsInterface
    {
        public void BasicMethod()
        {
            throw new NotImplementedException();
        }

        public string BasicProperty { get; set; }
        public event EventHandler BasicEvent;
    }
}