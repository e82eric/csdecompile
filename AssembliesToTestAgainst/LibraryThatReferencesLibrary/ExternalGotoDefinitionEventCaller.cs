using System;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionEventCaller
    {
        public void Run()
        {
            ExternalGotoDefinitionEventTarget obj = new ExternalGotoDefinitionEventTarget();
            obj.BasicEvent += ObjOnBasicEvent;
            obj.BasicEvent -= ObjOnBasicEvent;
        }

        private void ObjOnBasicEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}