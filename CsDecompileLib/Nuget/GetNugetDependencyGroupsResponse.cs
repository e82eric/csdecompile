using System.Collections.Generic;

namespace CsDecompileLib.Nuget;

public class GetNugetDependencyGroupsResponse
{
    public List<string> Groups { get; }

    public GetNugetDependencyGroupsResponse()
    {
        Groups = new List<string>();
    }
}