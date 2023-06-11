namespace CsDecompileLib.Nuget;

public class AddNugetPackageAndDependenciesRequest
{
    public string Identity { get; set; }
    public string Version { get; set; }
    public string DependencyGroup { get; set; }
    public string RootPackageDirectory { get; set; }
}