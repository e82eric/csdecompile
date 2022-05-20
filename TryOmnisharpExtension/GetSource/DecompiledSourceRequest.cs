using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension
{
    [OmniSharpEndpoint(Endpoints.DecompiledSource, typeof(DecompiledSourceRequest), typeof(DecompiledSourceResponse))]
    public class DecompiledSourceRequest : IRequest
    {
        public string AssemblyFilePath { get; set; }
        public string ContainingTypeFullName { get; set; }
    }
}