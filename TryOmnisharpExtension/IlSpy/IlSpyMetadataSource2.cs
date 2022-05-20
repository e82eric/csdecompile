namespace TryOmnisharpExtension.IlSpy
{
    public class IlSpyMetadataSource2
    {
        public string AssemblyName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string SourceText { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public string ContainingTypeFullName { get; set; }
        public string AssemblyFilePath { get; set; }
    }
}
