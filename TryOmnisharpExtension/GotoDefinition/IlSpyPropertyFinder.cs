using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
{
    [Export]
    public class IlSpyPropertyFinder : IlSpyDefinitionFinderBase<IProperty>
    {
        [ImportingConstructor]
        public IlSpyPropertyFinder(PropertyInTypeFinder propertyInTypeFinder, DecompilerFactory decompilerFactory): base(
                propertyInTypeFinder,decompilerFactory)
        {
        }
    }
}