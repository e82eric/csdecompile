using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
{
    [Export]
    public class IlSpyEventFinder : IlSpyDefinitionFinderBase<IEvent>
    {
        [ImportingConstructor]
        public IlSpyEventFinder(EventInTypeFinder eventInTypeFinder, DecompilerFactory decompilerFactory):base(
            eventInTypeFinder, decompilerFactory)
        {
        }
    }
}