using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

[Export]
public class IlSpyFieldFinder : IlSpyDefinitionFinderBase<IField>
{
    [ImportingConstructor]
    public IlSpyFieldFinder(FieldInTypeFinder fieldInTypeFinder, DecompilerFactory decompilerFactory):base(
        fieldInTypeFinder, decompilerFactory)
    {
    } 
}