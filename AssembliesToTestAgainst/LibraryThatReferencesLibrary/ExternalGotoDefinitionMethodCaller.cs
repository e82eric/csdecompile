using System;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionMethodCaller
    {
        public void Run()
        {
            new ExternalGotoDefinitionMethodTarget().ExternalBasicMethod();
            new ExternalGotoDefinitionMethodTarget().ExternalBasicMethod(string.Empty);
            new ExternalGotoDefinitionMethodTarget().ExternalBasicMethod(String.Empty, String.Empty);
        }
    }
}