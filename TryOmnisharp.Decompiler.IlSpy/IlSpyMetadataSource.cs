namespace IlSpy.Analyzer.Extraction
{
    public class IlSpyMetadataSource
    {
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string FileName { get; set; }
        public string SourceLine { get; set; }
        public string SourceText { get; set; }
        public string MemberName { get; set; }

        public int StatementLine { get; set; }

        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        
        public string ContainingTypeFullName { get; set; }
        public string AssemblyFilePath { get; set; }
    }
}