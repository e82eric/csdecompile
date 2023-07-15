using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionMethodWithParamsUser
    {
        public void Run()
        {
            new ExternalGotoDefinitionMethodWithParamsTarget().Run("Test");
            new ExternalGotoDefinitionMethodWithParamsTarget().Run("Test", "More");
            new ExternalGotoDefinitionMethodWithParamsTarget().Run("Test", "More", "Params");
        }
    }
}