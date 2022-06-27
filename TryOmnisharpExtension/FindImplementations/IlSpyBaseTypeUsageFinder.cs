using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

public class IlSpyBaseTypeUsageFinder : IlSpyUsagesFinderBase<ITypeDefinition>
{
    public IlSpyBaseTypeUsageFinder(
        TypesThatUseTypeAsBaseTypeMetadataScanner typesThatUseTypeUsedByAnalyzer,
        TypeUsedAsBaseTypeFinder typeUsedAsBaseTypeFinder,
        DecompilerFactory decompilerFactory) : base(
            decompilerFactory,
            typesThatUseTypeUsedByAnalyzer,
            typeUsedAsBaseTypeFinder)
    {
    }
}