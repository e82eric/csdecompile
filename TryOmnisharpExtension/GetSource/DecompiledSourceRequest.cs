using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension
{
    [OmniSharpEndpoint(Endpoints.DecompiledSource, typeof(DecompiledSourceRequest), typeof(DecompiledSourceResponse))]
    public class DecompiledSourceRequest : IRequest
    {
        public string AssemblyFilePath { get; set; }
        public string ContainingTypeFullName { get; set; }
        public string UsageType { get; set; }
        public string NamespaceName { get; set; }
        public string TypeName { get; set; }
        public string BaseTypeName { get; set; }
        public string MethodName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        
        public bool IsFromExternalAssembly { get; set; }
    }
}