using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.GetMembers;

[OmniSharpEndpoint(Endpoints.GetTypeMembers, typeof(GetTypeMembersRequest), typeof(GetTypeMembersResponse))]
public class GetTypeMembersRequest : DecompiledLocationRequest
{
}