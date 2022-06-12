using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.GetSource
{
    [OmniSharpEndpoint(Endpoints.DecompiledSource, typeof(DecompiledSourceRequest), typeof(DecompiledSourceResponse))]
    public class DecompiledSourceRequest : IRequest
    {
        public string AssemblyFilePath { get; set; }
        public string ContainingTypeFullName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public bool IsFromExternalAssembly { get; set; }
    }
}