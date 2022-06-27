using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

public class IlSpyMemberImplementationFinder : IlSpyUsagesFinderBase<IMember>
{
    public IlSpyMemberImplementationFinder(
        DecompilerFactory decompilerFactory,
        TypesThatUseMemberAsBaseTypeMetadataScanner typesThatUseMemberAsBaseTypeMetadataScanner,
        MemberOverrideInTypeFinder memberOverrideInTypeFinder) : base(
        decompilerFactory,
        typesThatUseMemberAsBaseTypeMetadataScanner,
        memberOverrideInTypeFinder)
    {
    }
}