using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages
{
    public class IlSpyTypeUsagesFinder : IlSpyUsagesFinderBase<ITypeDefinition>
    {
        public IlSpyTypeUsagesFinder(
            DecompilerFactory decompilerFactory,
            TypeUsedByTypeIlScanner typeUsedByTypeIlScanner,
            TypeUsedInTypeFinder usageFinder):base(
            decompilerFactory,
            typeUsedByTypeIlScanner,
            usageFinder) 
        {
        }
    }
}
