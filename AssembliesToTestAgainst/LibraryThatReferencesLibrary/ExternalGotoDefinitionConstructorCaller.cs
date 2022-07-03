using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionConstructorCaller
    {
        public void Run()
        {
            new ExternalGotoDefinitionConstructorTarget();
            new ExternalGotoDefinitionConstructorTarget(string.Empty);
            new ExternalGotoDefinitionConstructorTarget(string.Empty, string.Empty);
        }
    }
}