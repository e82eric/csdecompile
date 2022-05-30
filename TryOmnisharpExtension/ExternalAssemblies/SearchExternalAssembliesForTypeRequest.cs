using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.ExternalAssemblies;

[OmniSharpEndpoint(Endpoints.SearchExternalAssembliesForType, typeof(SearchExternalAssembliesForTypeRequest), typeof(SearchExternalAssembliesForTypeResponse))]
public class SearchExternalAssembliesForTypeRequest : IRequest
{
    public string TypeName { get; set; }
}