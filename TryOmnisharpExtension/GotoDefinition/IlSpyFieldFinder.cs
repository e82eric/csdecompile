using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

public class IlSpyFieldFinder : IlSpyDefinitionFinderBase<IField>
{
    public IlSpyFieldFinder(FieldInTypeFinder fieldInTypeFinder, DecompilerFactory decompilerFactory):base(
        fieldInTypeFinder, decompilerFactory)
    {
    } 
}