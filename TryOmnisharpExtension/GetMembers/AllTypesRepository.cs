using System.Collections.Generic;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GetMembers;

public class AllTypesRepository
{
    private readonly IDecompileWorkspace _workspace;
    private readonly AssemblyResolverFactory _assemblyResolverFactory;

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
        }
        
        return result;
    }
    
    public IEnumerable<DecompileInfo> GetAssemblyType(string assemblyFilePath)
    {
        var peFile = _workspace.GetAssembly(assemblyFilePath);
        var result = GetAssemblyTypes(peFile);
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
    
    private IList<DecompileInfo> GetAssemblyTypes(PEFile peFile)
    {
        var result = new List<DecompileInfo>();
        var typeDefinitions = peFile.Metadata.TypeDefinitions;
        foreach (var typeDefinitionHandle in typeDefinitions)
        {
            var typeDef = peFile.Metadata.GetTypeDefinition(typeDefinitionHandle);
            var foundTypeName = peFile.Metadata.GetString(typeDef.Name);
            if (foundTypeName != null)
            {
                var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile.Metadata);
                var fullName = fullTypeName.ReflectionName;

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
            }
        }

        return result;
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