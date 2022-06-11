using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

[Export]
public class IlSpyMemberImplementationFinder : IlSpyUsagesFinderBase<IMember>
{
    [ImportingConstructor]
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