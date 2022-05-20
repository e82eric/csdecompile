using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension;

[OmniSharpEndpoint(Endpoints.GetTypes, typeof(GetTypesRequest), typeof(GetTypesResponse))]
public class GetTypesRequest : IRequest
{
}