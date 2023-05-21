using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGetTypeMembersCaller
    {
        public void Run()
        {
            new ExternalGetTypeMembersTarget("param1");
        }
    }
}