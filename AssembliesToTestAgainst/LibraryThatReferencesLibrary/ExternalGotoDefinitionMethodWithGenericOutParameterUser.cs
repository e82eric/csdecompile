using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionMethodWithGenericOutParameterUser
    {
        public void Run()
        {
            new ExternalGotoDefinitionMethodWithGenericOutParameterTarget<string, object>().TryRun("test", out _);
        }
    }
}