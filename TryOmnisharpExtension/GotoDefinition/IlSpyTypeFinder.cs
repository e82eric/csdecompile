using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition
 {
     [Export]
     public class IlSpyTypeFinder : IlSpyDefinitionFinderBase<ITypeDefinition>
     {
         [ImportingConstructor]
         public IlSpyTypeFinder(TypeInTypeFinder typeInFinder, DecompilerFactory decompilerFactory) : base(
             typeInFinder, decompilerFactory)
         {
         }
     }
 }
