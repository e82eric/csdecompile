using System.Collections.Generic;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;

namespace CsDecompileLib.GetMembers;

public class AllTypesRepositoryByName
{
    private readonly IDecompileWorkspace _workspace;
    private readonly AssemblyResolverFactory _assemblyResolverFactory;

    public AllTypesRepositoryByName(IDecompileWorkspace workspace, AssemblyResolverFactory assemblyResolverFactory)
    {
        _workspace = workspace;
        _assemblyResolverFactory = assemblyResolverFactory;
    }

    public IEnumerable<DecompileInfo> GetAllTypes(string namespaceName, string typeName)
    {
        var projectPeFiles = _workspace.GetAssemblies();
        var alreadyAdded = new HashSet<string>();
        var alreadyCheckedPeFiles = new HashSet<string>();
        var result = new List<DecompileInfo>();
        foreach (var peFile in projectPeFiles)
        {
            var assemblyResolver = _assemblyResolverFactory.GetAssemblyResolver(peFile);
            SearchReferencingModules(
                peFile,
                alreadyCheckedPeFiles,
                assemblyResolver,
                namespaceName,
                typeName,
                alreadyAdded,
                result);
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
        string namespaceName,
        string typeName,
        HashSet<string> alreadyAddedTypes,
        IList<DecompileInfo> foundTypes)
    {
        var uniqueness = GetPeFileUniqueness(pefile);
        if (!alreadyChecked.Contains(uniqueness))
        {
            alreadyChecked.Add(uniqueness);
            AddTypeDefinitions(pefile, namespaceName, typeName, alreadyAddedTypes, foundTypes);

            foreach (var assemblyReference in pefile.AssemblyReferences)
            {
                var resolved = assemblyResolver.Resolve(assemblyReference);
                if (resolved != null)
                {
                    SearchReferencingModules(
                        resolved,
                        alreadyChecked,
                        assemblyResolver,
                        namespaceName,
                        typeName,
                        alreadyAddedTypes,
                        foundTypes);
                }
            }
        }
    }

    private void AddTypeDefinitions(PEFile peFile, string namespaceName, string typeName, HashSet<string> alreadyAdded,
        IList<DecompileInfo> result)
    {
        var typeDefinitions = peFile.Metadata.TypeDefinitions;
        foreach (var typeDefinitionHandle in typeDefinitions)
        {
            var typeDef = peFile.Metadata.GetTypeDefinition(typeDefinitionHandle);
            var typeDefNamespace = peFile.Metadata.GetString(typeDef.Namespace);
            var foundTypeName = peFile.Metadata.GetString(typeDef.Name);
            if (foundTypeName != null)
            {
                if (foundTypeName.Equals(typeName) && typeDefNamespace.Equals(namespaceName))
                {
                    foreach (var method in typeDef.GetMethods())
                    {
                        var methodDefinition = peFile.Metadata.GetMethodDefinition(method);
                        var currentMethodName = peFile.Metadata.GetString(methodDefinition.Name);
                        var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile.Metadata);
                        var fullName = fullTypeName.ReflectionName;
                        var shortName = fullTypeName.Name;

                        if (!alreadyAdded.Contains(fullName))
                        {
                            var decompileInfo = new DecompileInfo
                            {
                                ParentAssemblyFilePath = peFile.FileName,
                                AssemblyFilePath = peFile.FileName,
                                AssemblyName = peFile.FullName,
                                Column = 1,
                                ContainingTypeFullName = fullName,
                                ContainingTypeShortName = shortName,
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
}