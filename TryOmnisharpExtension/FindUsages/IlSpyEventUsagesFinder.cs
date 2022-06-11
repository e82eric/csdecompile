using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages
{
    [Export]
    public class IlSpyEventUsagesFinder : IlSpyUsagesFinderBase<IMember>
    {
        [ImportingConstructor]
        public IlSpyEventUsagesFinder(
            DecompilerFactory decompilerFactory,
            EventUsedByMetadataScanner eventUsedByMetadataScanner,
            MemberUsedInTypeFinder memberUsedInTypeFinder) : base(
            decompilerFactory, eventUsedByMetadataScanner, memberUsedInTypeFinder)
        {
        }
    }
}
