using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;

namespace CsDecompileLib.GetMembers;

public class IlSpyTypesInReferencesSearcher
{
    private readonly IDecompileWorkspace _workspace;
    private readonly AssemblyResolverFactory _assemblyResolverFactory;

    public IlSpyTypesInReferencesSearcher(
        IDecompileWorkspace workspace,
        AssemblyResolverFactory assemblyResolverFactory)
    {
        _workspace = workspace;
        _assemblyResolverFactory = assemblyResolverFactory;
    }

    public Task<IEnumerable<ResponseLocation>> GetAllTypes(Predicate<DecompileInfo> resultFilter)
    {
        var projectPeFiles = _workspace.GetAssemblies();
        var alreadyAdded = new HashSet<string>();
        var alreadyCheckedPeFiles = new HashSet<string>();
        var result = new List<ResponseLocation>();
        foreach (var peFile in projectPeFiles)
        {
            var assemblyResolver = _assemblyResolverFactory.GetAssemblyResolver(peFile);
            SearchReferencingModules(
                peFile,
                alreadyCheckedPeFiles,
                assemblyResolver,
                alreadyAdded,
                result,
                resultFilter);
        }

        return Task.FromResult<IEnumerable<ResponseLocation>>(result);
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
        HashSet<string> alreadyAddedTypes,
        IList<ResponseLocation> foundTypes,
        Predicate<DecompileInfo> resultFilter)
    {
        var uniqueness = GetPeFileUniqueness(pefile);
        if (!alreadyChecked.Contains(uniqueness))
        {
            alreadyChecked.Add(uniqueness);
            AddTypeDefinitions(pefile, alreadyAddedTypes, foundTypes, resultFilter);

            foreach (var assemblyReference in pefile.AssemblyReferences)
            {
                var resolved = assemblyResolver.Resolve(assemblyReference);
                if (resolved != null)
                {
                    SearchReferencingModules(
                        resolved,
                        alreadyChecked,
                        assemblyResolver,
                        alreadyAddedTypes,
                        foundTypes,
                        resultFilter);
                }
            }
        }
    }

    private void AddTypeDefinitions(
        PEFile peFile,
        HashSet<string> alreadyAdded,
        IList<ResponseLocation> result,
        Predicate<DecompileInfo> resultFilter)
    {
        var typeDefinitions = peFile.Metadata.TypeDefinitions;
        foreach (var typeDefinitionHandle in typeDefinitions)
        {
            var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile.Metadata);
            var fullName = fullTypeName.ReflectionName;
            if (!alreadyAdded.Contains(fullName))
            {
                var typeDef = peFile.Metadata.GetTypeDefinition(typeDefinitionHandle);
                var foundTypeName = peFile.Metadata.GetString(typeDef.Name);
                if (foundTypeName != null)
                {
                    var decompileInfo = PEMetadataHelper.MapTypeDefinitionToDecompileInfo(typeDefinitionHandle, peFile);
                    if (resultFilter(decompileInfo))
                    {
                        result.Add(decompileInfo);
                        alreadyAdded.Add(fullName);
                    }
                }
            }
        }
    }
}