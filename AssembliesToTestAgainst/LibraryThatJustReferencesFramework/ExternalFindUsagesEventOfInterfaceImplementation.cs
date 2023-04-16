using System;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesEventOfInterfaceImplementation : ExternalFindUsagesEventOfInterfaceInterface
    {
        public event EventHandler Event1;
    }
}