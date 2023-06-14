namespace CsDecompileLib.Nuget;

public class SearchNugetForLocationResponse : SearchNugetResponse
{
    public string ParentAssemblyName { get; set; }
    public int? ParentAssemblyMajorVersion { get; set; }
    public int? ParentAssemblyMinorVersion { get; set; }
    public int? ParentAssemblyBuildVersion { get; set; }
}