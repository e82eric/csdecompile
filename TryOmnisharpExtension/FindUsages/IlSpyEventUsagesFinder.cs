using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages
{
    public class IlSpyEventUsagesFinder : IlSpyUsagesFinderBase<IMember>
    {
        public IlSpyEventUsagesFinder(
            DecompilerFactory decompilerFactory,
            EventUsedByMetadataScanner eventUsedByMetadataScanner,
            MemberUsedInTypeFinder memberUsedInTypeFinder) : base(
            decompilerFactory, eventUsedByMetadataScanner, memberUsedInTypeFinder)
        {
        }
    }
}
