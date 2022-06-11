using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class IlSpyFieldUsagesFinder : IlSpyUsagesFinderBase<IMember>
{
    [ImportingConstructor]
    public IlSpyFieldUsagesFinder(
        FieldUsedByMetadataScanner fieldUsedByAnalyzer,
        MemberUsedInTypeFinder fieldInMemberBodyFinder,
        DecompilerFactory decompilerFactory) : base(
        decompilerFactory, fieldUsedByAnalyzer, fieldInMemberBodyFinder)
    {
    }
}