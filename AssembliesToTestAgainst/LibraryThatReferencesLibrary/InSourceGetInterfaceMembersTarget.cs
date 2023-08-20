using System.Collections.Generic;

namespace LibraryThatReferencesLibrary
{
    public interface InSourceGetInterfaceMembersTarget
    {
        void Method1();
        string Prop1 { get; set; }
        IEnumerable<string> Prop2 { get; set; }
    }
}