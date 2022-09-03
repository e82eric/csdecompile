namespace CsDecompileLib.GotoDefinition
{
    public class DecompileGotoDefinitionResponse
    {
        public bool IsDecompiled { get; set; }
        public ResponseLocation Location { get; set; }
        public string SourceText { get; set; }
        public ErrorDetails ErrorDetails { get; set; }
    }

    public class ErrorDetails
    {
        public string Message { get; set; }
    }
}