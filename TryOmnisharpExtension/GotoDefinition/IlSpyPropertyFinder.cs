using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class IlSpyPropertyFinder : IlSpyDefinitionFinderBase<IProperty>
    {
        public IlSpyPropertyFinder(
            PropertyInTypeFinder propertyInTypeFinder,
            TypeInTypeFinder typeInTypeFinder,
            DecompilerFactory decompilerFactory): base(
                propertyInTypeFinder, typeInTypeFinder, decompilerFactory)
        {
        }
    }
}