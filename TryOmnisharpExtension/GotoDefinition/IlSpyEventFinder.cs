using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class IlSpyEventFinder : IlSpyDefinitionFinderBase<IEvent>
    {
        public IlSpyEventFinder(
            EventInTypeFinder eventInTypeFinder,
            TypeInTypeFinder typeInTypeFinder,
            DecompilerFactory decompilerFactory):base(
            eventInTypeFinder, typeInTypeFinder, decompilerFactory)
        {
        }
    }
}