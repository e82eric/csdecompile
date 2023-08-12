using System.Collections.Generic;

namespace CsDecompileLib.GotoDefinition
{
    public class GotoDefinitionResponse
    {
        public ResponseLocation Location { get; set; }
        public string SourceText { get; set; }

        public IEnumerable<ResponseLocation> Locations { get; set; }
    }
}