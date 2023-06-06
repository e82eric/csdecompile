using System.Collections.Generic;

namespace CsDecompileLib.Nuget;

public class GetNugetVersionsResponse
{
    public GetNugetVersionsResponse()
    {
        Packages = new List<Package>();
    }

    public IList<Package> Packages { get; }
}