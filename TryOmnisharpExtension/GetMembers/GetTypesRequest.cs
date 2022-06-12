using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.GetMembers;

[OmniSharpEndpoint(Endpoints.GetTypes, typeof(GetTypesRequest), typeof(GetTypesResponse))]
public class GetTypesRequest : IRequest
{
    public string SearchString { get; set; }
}