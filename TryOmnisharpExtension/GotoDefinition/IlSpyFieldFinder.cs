using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

public class IlSpyFieldFinder : IlSpyDefinitionFinderBase<IField>
{
    public IlSpyFieldFinder(
        FieldInTypeFinder fieldInTypeFinder,
        TypeInTypeFinder typeInTypeFinder,
        DecompilerFactory decompilerFactory):base(
        fieldInTypeFinder, typeInTypeFinder, decompilerFactory)
    {
    } 
}