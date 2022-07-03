using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionDefaultConstructorCaller
    {
        public void Run()
        {
            new ExternalGotoDefinitionDefaultConstructorTarget();
        }
    }
}