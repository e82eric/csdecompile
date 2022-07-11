using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindUsagesEnumCaller
    {
        public void Run()
        {
            var t = ExternalFindUsagesEnumTarget.Type1;
            var t2 = ExternalFindUsagesEnumTarget.Type2;
            var t3 = ExternalFindUsagesEnumTarget.Type3;
            ExternalFindUsagesEnumUser u = null;
        }
    }
}