using ExternalGotoDefinitionNamespaceTarget;
using ExternalGotoDefinitionNamespaceTarget.SubNamespace;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionNamespaceUser
    {
        public void Run()
        {
            Class1 a;
            SubClass1 b;
        }
    }
}