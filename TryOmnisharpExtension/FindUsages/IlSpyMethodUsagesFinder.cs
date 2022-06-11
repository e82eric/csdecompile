using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages
{
    [Export]
    public class IlSpyMethodUsagesFinder : IlSpyUsagesFinderBase<IMember>
    {
        [ImportingConstructor]
        public IlSpyMethodUsagesFinder(
            DecompilerFactory decompilerFactory,
            MethodUsedByMetadataScanner methodUsedByScanner,
            MemberUsedInTypeFinder entityUsedInTypeFinder):base(
            decompilerFactory, methodUsedByScanner, entityUsedInTypeFinder)
        {
        }
    }
}
