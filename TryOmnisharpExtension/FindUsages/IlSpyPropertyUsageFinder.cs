using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages
{
    public class IlSpyPropertyUsagesFinder : IlSpyUsagesFinderBase<IMember>
    {
        public IlSpyPropertyUsagesFinder(
            PropertyUsedByMetadataScanner memberUsedByScanner,
            MemberUsedInTypeFinder memberUsedInTypeFinder,
            DecompilerFactory decompilerFactory) :base(
                decompilerFactory, memberUsedByScanner, memberUsedInTypeFinder)
        {
        }
    }
}