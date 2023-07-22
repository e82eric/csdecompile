using System.Collections.Generic;

namespace CsDecompileLib.GetMembers;

public class MemberSearchRequest
{
    public IEnumerable<string> AssemblySearchStrings { get; set; }
    public string MemberSearchString { get; set; }
}