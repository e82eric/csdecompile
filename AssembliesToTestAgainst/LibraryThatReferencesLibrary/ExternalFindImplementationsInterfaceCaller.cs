using System;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindImplementationsInterfaceCaller
    {
        public void Run()
        {
            ExternalFindImplementationsInterface a = null;
            a.BasicMethod();
            a.BasicProperty = "0";
            var b = a.BasicProperty;
            a.BasicEvent += AOnBaicEvent;
            a.BasicEvent -= AOnBaicEvent;
        }

        private void AOnBaicEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}