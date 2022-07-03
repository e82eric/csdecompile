using System;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindUsagesMethodCaller
    {
        public void Run()
        {
            new ExternalFindUsagesMethodTarget().ExternalBasicMethod();
            new ExternalFindUsagesMethodTarget().ExternalBasicMethod(string.Empty);
            new ExternalFindUsagesMethodTarget().ExternalBasicMethod(String.Empty, String.Empty);
        }
    }
}