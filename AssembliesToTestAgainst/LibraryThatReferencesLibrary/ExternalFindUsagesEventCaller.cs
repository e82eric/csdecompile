using System;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindUsagesEventCaller
    {
        public void Run()
        {
            new ExternalFindUsagesEventTarget().ExternalBasicEvent += OnExternalBasicEvent;
        }

        private void OnExternalBasicEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}