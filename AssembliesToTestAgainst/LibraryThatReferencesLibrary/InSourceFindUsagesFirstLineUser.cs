using InSourceFindUsagesFirstLineAlias = LibraryThatReferencesLibrary.InSourceFindUsagesFirstLineTarget;

namespace LibraryThatReferencesLibrary
{
    public struct InSourceFindUsagesFirstLineUser
    {
        public void Run()
        {
            InSourceFindUsagesFirstLineTarget a;
        }
    }
}
