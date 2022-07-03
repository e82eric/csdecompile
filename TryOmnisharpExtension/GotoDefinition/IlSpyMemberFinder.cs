using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

public class IlSpyMemberFinder : IlSpyDefinitionFinderBase<IMethod>
{
    public IlSpyMemberFinder(
        MethodInTypeFinder methodInTypeFinder,
        TypeInTypeFinder typeInTypeFinder,
        DecompilerFactory decompilerFactory): base(
        methodInTypeFinder, typeInTypeFinder, decompilerFactory)
    {
    } 
}