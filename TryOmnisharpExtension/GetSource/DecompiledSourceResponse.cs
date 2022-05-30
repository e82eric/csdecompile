namespace TryOmnisharpExtension
{
    public class DecompiledSourceResponse
    {
        public bool IsDecompiled { get; set; }
        public bool IsFromExternalAssemblies { get; set; }
        public string AssemblyFilePath { get; set; }
        public string ContainingTypeFullName { get; set; }
        public string SourceText { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}