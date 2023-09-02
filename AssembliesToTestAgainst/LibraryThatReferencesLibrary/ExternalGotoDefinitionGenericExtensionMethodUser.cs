using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionGenericExtensionMethodUser
    {
        public void Run(ExternalGotoDefinitionGenericExtensionMethodTarget<string> a)
        {
            a.Run();
        }
    }
}