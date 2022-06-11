using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class AllTypesRepository
{
    private readonly IDecompileWorkspace _workspace;

    [ImportingConstructor]
    public AllTypesRepository(
        IDecompileWorkspace workspace)
    {
        _workspace = workspace;
    }

    public IEnumerable<DecompileInfo> GetAllTypes(string searchString)
    {
        var projectPeFiles = _workspace.GetAssemblies();

        var alreadyAdded = new HashSet<string>();
        var result = new List<DecompileInfo>();
        foreach (var peFile in projectPeFiles)
        {
            var typeDefinitions = peFile.Metadata.TypeDefinitions;
            foreach (var typeDefinitionHandle in typeDefinitions)
            {
                var typeDef = peFile.Metadata.GetTypeDefinition(typeDefinitionHandle);
                var foundTypeName = peFile.Metadata.GetString(typeDef.Name);
                if (foundTypeName != null)
                {
                    if (foundTypeName.Contains(searchString))
                    {
                        var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile.Metadata);
                        var fullName = fullTypeName.ReflectionName;

                        if (!alreadyAdded.Contains(fullName))
                        {
                            var namespaceName = peFile.Metadata.GetString(typeDef.Namespace);
                            var decompileInfo = new DecompileInfo()
                            {
                                AssemblyFilePath = peFile.FileName,
                                AssemblyName = peFile.FullName,
                                Column = 1,
                                ContainingTypeFullName = fullName,
                                EndColumn = 1,
                                Line = 1,
                                SourceText = fullName,
                                NamespaceName = namespaceName,
                                StartColumn = 1,
                                UsageType = UsageTypes.Type
                            };
                            result.Add(decompileInfo);
                            alreadyAdded.Add(fullName);
                        }
                    }
                }
            }
        }
        
        return result;
    }
}