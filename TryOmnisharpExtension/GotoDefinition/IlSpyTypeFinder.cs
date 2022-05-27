using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy
 {
     [Export]
     public class IlSpyTypeFinder
     {
         private readonly TypenFinder2 _typeInFinder;

         [ImportingConstructor]
         public IlSpyTypeFinder(TypenFinder2 typeInFinder)
         {
             _typeInFinder = typeInFinder;
         }
         
         public async Task<(IlSpyMetadataSource2, string source)> FindDefinitionFromSymbolName(ITypeDefinition typeToFindDefinitionFor)
         {
             var rootTypeOfTypeToFindDefinitionFor = SymbolHelper.FindContainingType(typeToFindDefinitionFor);

             var result = await FindTypeDefinition( typeToFindDefinitionFor, rootTypeOfTypeToFindDefinitionFor);
             return result;
         }

         private async Task<(IlSpyMetadataSource2, string source)> FindTypeDefinition(
             ITypeDefinition typeToFindDefinitionFor,
             ITypeDefinition rootTypeOfTypeToFindDefinitionFor)
         {
             var (locationOfDefinition, sourceOfDefinition) = await _typeInFinder.Find(
                typeToFindDefinitionFor,
                typeToFindDefinitionFor.MetadataToken,
                rootTypeOfTypeToFindDefinitionFor);
 
             var metadataSource = new IlSpyMetadataSource2
             {
                 AssemblyName = rootTypeOfTypeToFindDefinitionFor.ParentModule.AssemblyName,
                 Column = locationOfDefinition.StartLocation.Column,
                 Line = locationOfDefinition.StartLocation.Line,
                 SourceText = $"{typeToFindDefinitionFor.ReflectionName} {locationOfDefinition.Statement.ToString().Replace("\r\n", "")}",
                 StartColumn = locationOfDefinition.StartLocation.Column,
                 EndColumn = locationOfDefinition.EndLocation.Column,
                 ContainingTypeFullName = rootTypeOfTypeToFindDefinitionFor.ReflectionName,
             };
 
             return (metadataSource, sourceOfDefinition);
         }
     }
 }
