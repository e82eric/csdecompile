namespace CsDecompileLib.Nuget;

public class AddNugetPackageAndDependenciesRequest
{
    public NugetSource[] NugetSources { get; set; }
    public string PackageId { get; set; }
    public string PackageVersion { get; set; }
    public string DependencyGroup { get; set; }
    public string RootPackageDirectory { get; set; }
}