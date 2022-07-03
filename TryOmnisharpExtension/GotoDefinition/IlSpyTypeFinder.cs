using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
 {
     public class IlSpyTypeFinder : IlSpyDefinitionFinderBase<ITypeDefinition>
     {
         public IlSpyTypeFinder(
             TypeInTypeFinder typeInFinder,
             TypeInTypeFinder typeInTypeFinder,
             DecompilerFactory decompilerFactory) : base(
             typeInFinder, typeInFinder, decompilerFactory)
         {
         }
     }
 }
