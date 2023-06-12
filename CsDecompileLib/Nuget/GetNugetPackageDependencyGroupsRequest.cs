namespace CsDecompileLib.Nuget;

public class GetNugetPackageDependencyGroupsRequest
{
    public string PackageId { get; set; }
    public string PackageVersion { get; set; }
}