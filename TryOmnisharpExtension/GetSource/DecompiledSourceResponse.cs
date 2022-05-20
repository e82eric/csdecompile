namespace TryOmnisharpExtension
{
    public class DecompiledSourceResponse
    {
        public bool IsDecompiled { get; set; }
        public string AssemblyFilePath { get; set; }
        public string ContainingTypeFullName { get; set; }
        public string SourceText { get; set; }
    }
}