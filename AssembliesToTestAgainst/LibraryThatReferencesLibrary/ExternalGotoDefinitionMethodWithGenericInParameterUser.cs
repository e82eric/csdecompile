using System;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionMethodWithGenericInParameterUser
    {
        public void Run()
        {
            var o = new Object();
            new ExternalGotoDefinitionMethodWithGenericInParameterTarget<string, object>().TryRun("test", in o);
        }
    }
}