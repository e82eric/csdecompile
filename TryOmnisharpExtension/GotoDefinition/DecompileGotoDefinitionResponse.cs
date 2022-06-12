namespace TryOmnisharpExtension.GotoDefinition
{
    public class DecompileGotoDefinitionResponse
    {
        public bool IsDecompiled { get; set; }
        public ResponseLocation Location { get; set; }
        public string SourceText { get; set; }
    }
}