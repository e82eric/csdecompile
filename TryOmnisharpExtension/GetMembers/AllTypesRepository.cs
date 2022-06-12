using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using TryOmnisharpExtension.IlSpy;
using AssemblyReference = ICSharpCode.Decompiler.Metadata.AssemblyReference;

namespace TryOmnisharpExtension.GetMembers;

[Export]
public class AllTypesRepository
{
    private readonly IDecompileWorkspace _workspace;
    private readonly AssemblyResolverFactory _assemblyResolverFactory;

    [ImportingConstructor]
    public AllTypesRepository(IDecompileWorkspace workspace, AssemblyResolverFactory assemblyResolverFactory)
    {
        _workspace = workspace;
        _assemblyResolverFactory = assemblyResolverFactory;
    }

    public IEnumerable<DecompileInfo> GetAllTypes(string searchString)
    {
        var projectPeFiles = _workspace.GetAssemblies();
        var alreadyAdded = new HashSet<string>();
        var alreadyCheckedPeFiles = new HashSet<string>();
        var result = new List<DecompileInfo>();
        foreach (var peFile in projectPeFiles)
        {
            var assemblyResolver = _assemblyResolverFactory.GetAssemblyResolver(peFile);
            SearchReferencingModules(peFile, alreadyCheckedPeFiles, assemblyResolver, searchString, alreadyAdded, result);
            // var typeDefinitions = peFile.TypeDefinitions;
            // foreach (var typeDefinitionHandle in typeDefinitions)
            // {
            //     var typeDef = peFile.GetTypeDefinition(typeDefinitionHandle);
            //     var foundTypeName = peFile.GetString(typeDef.Name);
            //     if (foundTypeName != null)
            //     {
            //         if (foundTypeName.Contains(searchString))
            //         {
            //             var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile);
            //             var fullName = fullTypeName.ReflectionName;
            //
            //             if (!alreadyAdded.Contains(fullName))
            //             {
            //                 var namespaceName = peFile.GetString(typeDef.Namespace);
            //                 peFile.GetModuleReference(typeDef..)
            //                 var assemblyFilePath = peFile.ManifestResources.FileName;
            //                 var decompileInfo = new DecompileInfo
            //                 {
            //                     AssemblyFilePath = peFile.FileName,
            //                     AssemblyName = peFile.FullName,
            //                     Column = 1,
            //                     ContainingTypeFullName = fullName,
            //                     EndColumn = 1,
            //                     Line = 1,
            //                     SourceText = fullName,
            //                     NamespaceName = namespaceName,
            //                     StartColumn = 1,
            //                     // UsageType = UsageTypes.Type
            //                 };
            //                 result.Add(decompileInfo);
            //                 alreadyAdded.Add(fullName);
            //             }
            //         }
            //     }
            // }
        }
        
        return result;
    }
    

    private string GetPeFileUniqueness(PEFile peFile)
    {
        var uniqueness = peFile.FullName + "|" + peFile.Metadata.MetadataVersion;
        return uniqueness;
    }
        
    private void SearchReferencingModules(
        PEFile pefile,
        HashSet<string> alreadyChecked,
        IAssemblyResolver assemblyResolver,
        string searchString,
        HashSet<string> alreadyAddedTypes,
        IList<DecompileInfo> foundTypes)
    {
        var uniqueness = GetPeFileUniqueness(pefile);
        if (!alreadyChecked.Contains(uniqueness))
        {
            alreadyChecked.Add(uniqueness);
            AddTypeDefinitions(pefile, searchString, alreadyAddedTypes, foundTypes);

            foreach (var assemblyReference in pefile.AssemblyReferences)
            {
                var resolved = assemblyResolver.Resolve(assemblyReference);
                if (resolved != null)
                {
                    SearchReferencingModules(
                        resolved,
                        alreadyChecked,
                        assemblyResolver,
                        searchString,
                        alreadyAddedTypes,
                        foundTypes);
                }
            }
        }
    }

    private void AddTypeDefinitions(PEFile peFile, string searchString, HashSet<string> alreadyAdded, IList<DecompileInfo> result)
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
                        var decompileInfo = new DecompileInfo
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
                            // UsageType = UsageTypes.Type
                        };
                        result.Add(decompileInfo);
                        alreadyAdded.Add(fullName);
                    }
                }
            }
        }
    }
}