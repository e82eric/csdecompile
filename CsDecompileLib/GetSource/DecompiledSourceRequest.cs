namespace CsDecompileLib.GetSource
{
    public class DecompiledSourceRequest
    {
        public string AssemblyFilePath { get; set; }
        public string ContainingTypeFullName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}