using System.Collections.Generic;

namespace CsDecompileLib.Nuget;

public class GetNugetPackageVersionsResponse
{
    public GetNugetPackageVersionsResponse()
    {
        Packages = new List<Package>();
    }

    public IList<Package> Packages { get; }
    public string PackageId { get; set; }
    public int? ParentAssemblyMajorVersion { get; set; }
    public int? ParentAssemblyMinorVersion { get; set; }
    public int? ParentAssemblyBuildVersion { get; set; }
    
    public NugetSource[] NugetSources { get; set; }
}