using System.Collections.Generic;

namespace CsDecompileLib.Nuget;

public class SearchNugetResponse
{
    public SearchNugetResponse()
    {
        Packages = new List<Package>();
    }

    public IList<Package> Packages { get; }
    public string SearchString { get; set; }
}