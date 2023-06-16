namespace CsDecompileLib.Nuget;

public class GetNugetPackageVersionsRequest
{
    public string PackageId { get; set; }
    public int? ParentAssemblyMajorVersion { get; set; }
    public int? ParentAssemblyMinorVersion { get; set; }
    public int? ParentAssemblyBuildVersion { get; set; }
    public string[] NugetSources { get; set; }
}