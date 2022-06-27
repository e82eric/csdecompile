using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class IlSpyEventFinder : IlSpyDefinitionFinderBase<IEvent>
    {
        public IlSpyEventFinder(EventInTypeFinder eventInTypeFinder, DecompilerFactory decompilerFactory):base(
            eventInTypeFinder, decompilerFactory)
        {
        }
    }
}