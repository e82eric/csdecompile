using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages
{
    [Export]
    public class IlSpyTypeUsagesFinder : IlSpyUsagesFinderBase<ITypeDefinition>
    {
        [ImportingConstructor]
        public IlSpyTypeUsagesFinder(
            DecompilerFactory decompilerFactory,
            TypeUsedByTypeIlScanner typeUsedByTypeIlScanner,
            TypeUsedInTypeFinder3 typeUsedInTypeFinder2):base(
            decompilerFactory,
            typeUsedByTypeIlScanner,
            typeUsedInTypeFinder2) 
        {
        }
    }
}
