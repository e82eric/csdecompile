using System.Collections.Generic;

namespace CsDecompileLib.Nuget;

public class SearchNugetRequest
{
    public string SearchString { get; set; }
    public string[] NugetSources { get; set; }
}