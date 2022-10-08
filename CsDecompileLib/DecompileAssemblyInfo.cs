namespace CsDecompileLib;

public class DecompileAssemblyInfo : ResponseLocation
{
    public string AssemblyName { get; set; }
    public string AssemblyFilePath { get; set; }
    public string FileName { get; set; }
    public override LocationType Type => LocationType.DecompiledAssembly;
}