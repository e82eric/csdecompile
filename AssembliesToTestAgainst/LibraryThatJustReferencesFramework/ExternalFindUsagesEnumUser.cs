namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesEnumUser
    {
        public void Run()
        {
            var a = ExternalFindUsagesEnumTarget.Type1;
            var b = ExternalFindUsagesEnumTarget.Type2;
            var c = ExternalFindUsagesEnumTarget.Type3;
        }
    }
}