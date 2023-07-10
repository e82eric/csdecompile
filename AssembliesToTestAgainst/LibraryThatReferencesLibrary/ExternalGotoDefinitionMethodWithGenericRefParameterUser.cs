using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionMethodWithGenericRefParameterUser
    {
        public void Run()
        {
            object a = "";
            new ExternalGotoDefinitionMethodWithGenericRefParameterTarget<string, object>().TryRun("test", ref a);
        }
    }
}