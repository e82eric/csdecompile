using CsDecompileLib.Nuget;

namespace CsDecompileLib;

public class NugetDecompiledLocationRequest : DecompiledLocationRequest
{
    public NugetSource[] NugetSources { get; set; }
}