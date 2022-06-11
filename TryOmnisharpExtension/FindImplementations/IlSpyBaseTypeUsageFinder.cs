using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

[Export]
public class IlSpyBaseTypeUsageFinder : IlSpyUsagesFinderBase<ITypeDefinition>
{
    [ImportingConstructor]
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