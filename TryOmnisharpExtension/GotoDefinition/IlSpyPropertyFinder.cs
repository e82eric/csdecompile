using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class IlSpyPropertyFinder : IlSpyDefinitionFinderBase<IProperty>
    {
        public IlSpyPropertyFinder(PropertyInTypeFinder propertyInTypeFinder, DecompilerFactory decompilerFactory): base(
                propertyInTypeFinder,decompilerFactory)
        {
        }
    }
}