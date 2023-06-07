namespace CsDecompileLib.Nuget;

public class GetNugetDependenciesRequest
{
    public string Identity { get; set; }
    public string Version { get; set; }
    public string DependencyGroup { get; set; }
}