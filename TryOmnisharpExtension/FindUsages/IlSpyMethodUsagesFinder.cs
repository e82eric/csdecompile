using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages
{
    public class IlSpyMethodUsagesFinder : IlSpyUsagesFinderBase<IMember>
    {
        public IlSpyMethodUsagesFinder(
            DecompilerFactory decompilerFactory,
            MethodUsedByMetadataScanner methodUsedByScanner,
            MemberUsedInTypeFinder entityUsedInTypeFinder):base(
            decompilerFactory, methodUsedByScanner, entityUsedInTypeFinder)
        {
        }
    }
}
