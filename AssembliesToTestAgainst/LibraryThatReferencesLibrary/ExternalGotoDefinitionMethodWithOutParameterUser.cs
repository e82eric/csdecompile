using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionMethodWithOutParameterUser
    {
        public void Run()
        {
            new ExternalGotoDefinitionMethodWithOutParameterTarget().TryRun("test", out _);
        }
    }
}