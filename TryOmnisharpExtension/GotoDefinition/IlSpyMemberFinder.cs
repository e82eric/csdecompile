using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

public class IlSpyMemberFinder : IlSpyDefinitionFinderBase<IMethod>
{
    public IlSpyMemberFinder(MethodInTypeFinder methodInTypeFinder, DecompilerFactory decompilerFactory): base(
        methodInTypeFinder, decompilerFactory)
    {
    } 
}