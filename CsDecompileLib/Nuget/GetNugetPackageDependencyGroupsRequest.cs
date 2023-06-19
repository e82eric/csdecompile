namespace CsDecompileLib.Nuget;

public class GetNugetPackageDependencyGroupsRequest
{
    public NugetSource[] NugetSources { get; set; }
    public string PackageId { get; set; }
    public string PackageVersion { get; set; }
}