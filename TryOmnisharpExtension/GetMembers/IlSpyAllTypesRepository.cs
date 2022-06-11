using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using TryOmnisharpExtension.ExternalAssemblies;

namespace TryOmnisharpExtension.GetMembers;

[Export]
public class IlSpyAllTypesRepository
{
    private readonly ExternalAssembliesWorkspace _externalAssembliesWorkspace;

    [ImportingConstructor]
    public IlSpyAllTypesRepository(ExternalAssembliesWorkspace externalAssembliesWorkspace)
    {
        _externalAssembliesWorkspace = externalAssembliesWorkspace;
    }

    public IEnumerable<DecompileInfo> GetAllTypes(string typeName)
    {
        var directoriesToSearch = _externalAssembliesWorkspace.GetDirectories();
        var result = new List<DecompileInfo>();
        foreach (var directoryInfo in directoriesToSearch)
        {
            var allDlls = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories);

            var flatModules = new List<PEFile>();
            foreach (var dll in allDlls)
            {
                try
                {
                    var apeFile = new PEFile(dll.FullName);
                    flatModules.Add(apeFile);
                }
                catch (Exception)
                {
                }
            }

            foreach (var peFile in flatModules)
            {
                var typeDefinitions = peFile.Metadata.TypeDefinitions;
                foreach (var typeDefinitionHandle in typeDefinitions)
                {
                    var typeDef = peFile.Metadata.GetTypeDefinition(typeDefinitionHandle);
                    var foundTypeName = peFile.Metadata.GetString(typeDef.Name);
                    if (foundTypeName.ToLower().Contains(typeName.ToLower()))
                    {
                        var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile.Metadata);
                        var fullName = fullTypeName.ReflectionName;

                        var decompileInfo = new DecompileInfo
                        {
                            AssemblyFilePath = peFile.FileName,
                            AssemblyName = peFile.FullName,
                            Column = 1,
                            ContainingTypeFullName = fullName,
                            EndColumn = 1,
                            Line = 1,
                            SourceText = fullName,
                            StartColumn = 1,
                            IsFromExternalAssembly = true,
                            NamespaceName = peFile.Metadata.GetString(typeDef.Namespace),
                            UsageType = UsageTypes.Type
                        };
                        result.Add(decompileInfo);
                    }
                }
            }
        }
        
        return result;
    }
}
