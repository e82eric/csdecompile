using System;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesMethodUser
    {
        public void Run()
        {
            new ExternalFindUsagesMethodTarget().ExternalBasicMethod();
            Run1(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);
            new ExternalFindUsagesMethodTarget().ExternalBasicMethod(String.Empty);
            Run2(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);
            new ExternalFindUsagesMethodTarget().ExternalBasicMethod(String.Empty, String.Empty);
            Run3(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);
        }
        private void Run1(Action action)
        {
        }
        private void Run2(Action<string> action)
        {
        }
        private void Run3(Action<string, string> action)
        {
        }
    }
}