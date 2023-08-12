using InSourceGotoDefinitionNamespaceTarget;
using InSourceGotoDefinitionNamespaceTarget.SubNamespace;

namespace LibraryThatReferencesLibrary
{
    public class InSourceGotoDefinitionNamespaceUser
    {
        public void Run()
        {
            InSourceClass1 a;
            SubInSourceClass1 b;
        }
    }
}