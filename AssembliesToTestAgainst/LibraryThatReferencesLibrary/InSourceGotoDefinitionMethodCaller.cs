using System;

namespace LibraryThatReferencesLibrary
{
    public class InSourceGotoDefinitionMethodCaller
    {
        public void Run()
        {
            new InSourceGotoDefinitionMethodTarget().BasicMethod();
            new InSourceGotoDefinitionMethodTarget().BasicMethod(string.Empty);
            new InSourceGotoDefinitionMethodTarget().BasicMethod(string.Empty, String.Empty);
        }
    }
}