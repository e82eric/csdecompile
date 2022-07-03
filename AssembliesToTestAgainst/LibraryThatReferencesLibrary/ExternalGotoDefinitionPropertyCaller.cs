using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionPropertyCaller
    {
        public void Run()
        {
            new ExternalGotoDefinitionPropertyTarget().ExternalBasicProperty = "0";
            var a = new ExternalGotoDefinitionPropertyTarget().ExternalBasicProperty;
        }
    }
}