using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
 {
     public class IlSpyTypeFinder : IlSpyDefinitionFinderBase<ITypeDefinition>
     {
         public IlSpyTypeFinder(TypeInTypeFinder typeInFinder, DecompilerFactory decompilerFactory) : base(
             typeInFinder, decompilerFactory)
         {
         }
     }
 }
