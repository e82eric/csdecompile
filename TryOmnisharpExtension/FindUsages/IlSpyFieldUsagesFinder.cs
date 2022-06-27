using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

public class IlSpyFieldUsagesFinder : IlSpyUsagesFinderBase<IMember>
{
    public IlSpyFieldUsagesFinder(
        FieldUsedByMetadataScanner fieldUsedByAnalyzer,
        MemberUsedInTypeFinder fieldInMemberBodyFinder,
        DecompilerFactory decompilerFactory) : base(
        decompilerFactory, fieldUsedByAnalyzer, fieldInMemberBodyFinder)
    {
    }
}