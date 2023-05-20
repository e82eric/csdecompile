using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    [ExternalGotoDefinition]
    public class ExternalGotoDefinitionAttributeUser
    {
        [ExternalGotoDefinition]
        public void Method1()
        {
        }
        
        [ExternalGotoDefinition]
        public string Prop1 { get; set; }
    }
}