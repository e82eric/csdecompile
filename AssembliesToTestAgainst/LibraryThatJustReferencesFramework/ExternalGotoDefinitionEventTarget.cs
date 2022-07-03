using System;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionEventTarget
    {
        public event EventHandler BasicEvent;

        public void Run()
        {
            BasicEvent(this, EventArgs.Empty);
        }
    }
}