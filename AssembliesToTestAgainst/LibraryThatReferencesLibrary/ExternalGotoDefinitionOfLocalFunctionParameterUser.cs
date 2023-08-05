using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionOfLocalFunctionParameterUser
    {
        public void Run()
        {
            ExternalGotoDefinitionOfLocalFunctionParameterTarget a;
        }
    }
}