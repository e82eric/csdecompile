using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionOfGenericPropertyUser
    {
        public void Run()
        {
            var a = new ExternalGotoDefinitionOfGenericPropertyTarget<string>();
            var b = a.Values;
        }
    }
}