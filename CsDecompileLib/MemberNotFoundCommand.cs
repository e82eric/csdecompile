using System.Threading.Tasks;

namespace CsDecompileLib;

// Roslyn resolved the symbol under the cursor, but it could not be matched to anything in the
// decompiled assembly. This is reported instead of guessing a nearby location.
class MemberNotFoundCommand<T> : INavigationCommand<T>
{
    private readonly string _memberName;

    public MemberNotFoundCommand(string memberName)
    {
        _memberName = memberName;
    }

    public Task<ResponsePacket<T>> Execute()
    {
        var response = new ResponsePacket<T>()
        {
            Message = $"MEMBER_NOT_FOUND {_memberName}",
            Success = false
        };
        return Task.FromResult(response);
    }
}
