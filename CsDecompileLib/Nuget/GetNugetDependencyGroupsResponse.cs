using System.Collections.Generic;

namespace CsDecompileLib.Nuget;

public class GetNugetDependencyGroupsResponse
{
    public List<string> Groups { get; }

    public GetNugetDependencyGroupsResponse()
    {
        Groups = new List<string>();
    }
    
    public string PackageId { get; set; }
    public string PackageVersion { get; set; }
}