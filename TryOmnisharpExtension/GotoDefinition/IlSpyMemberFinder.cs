using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

[Export]
public class IlSpyMemberFinder : IlSpyDefinitionFinderBase<IMethod>
{
    [ImportingConstructor]
    public IlSpyMemberFinder(MethodInTypeFinder methodInTypeFinder, DecompilerFactory decompilerFactory): base(
        methodInTypeFinder, decompilerFactory)
    {
    } 
}