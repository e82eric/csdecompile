using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages
{
    [Export]
    public class IlSpyPropertyUsagesFinder : IlSpyUsagesFinderBase<IMember>
    {
        [ImportingConstructor]
        public IlSpyPropertyUsagesFinder(
            PropertyUsedByMetadataScanner memberUsedByScanner,
            MemberUsedInTypeFinder memberUsedInTypeFinder,
            DecompilerFactory decompilerFactory) :base(
                decompilerFactory, memberUsedByScanner, memberUsedInTypeFinder)
        {
        }
    }
}