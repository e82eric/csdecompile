using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindUsagesMethodWithParamsUser
    {
        public void Run()
        {
            new ExternalFindUsagesMethodWithParamsTarget().Run("Test");
            new ExternalFindUsagesMethodWithParamsTarget().Run("Test", "More");
            new ExternalFindUsagesMethodWithParamsTarget().Run("Test", "More", "Params");
        }
    }
}