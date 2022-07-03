using System;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesEventCaller
    {
        public void Run()
        {
            var obj = new ExternalFindUsagesEventTarget();
            obj.ExternalBasicEvent += ObjOnExternalBasicEvent;
            obj.ExternalBasicEvent -= ObjOnExternalBasicEvent;
        }

        private void ObjOnExternalBasicEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}