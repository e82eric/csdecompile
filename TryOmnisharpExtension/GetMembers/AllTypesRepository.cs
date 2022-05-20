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
    private readonly DecompileWorkspace _workspace;
    private readonly AssemblyResolverFactory _assemblyResolverFactory;

    [ImportingConstructor]
    public AllTypesRepository(
        DecompileWorkspace workspace,
        AssemblyResolverFactory assemblyResolverFactory)
    {
        _workspace = workspace;
        _assemblyResolverFactory = assemblyResolverFactory;
    }

    private async Task CollectPeFiles(PEFile currentFile, List<PEFile> result, IAssemblyResolver assemblyResolver)
    {
        if (currentFile.AssemblyReferences.Any())
        {
            foreach (var assemblyReference in currentFile.AssemblyReferences)
            {
                var peFile = await assemblyResolver.ResolveAsync(assemblyReference);
                if (!result.Contains(peFile))
                {
                    result.Add(peFile);
                }

                await CollectPeFiles(peFile, result, assemblyResolver);
            }
        }
    }

    public async Task<IEnumerable<DecompileInfo>> GetAllTypes()
    {
        var projectPeFiles = await _workspace.GetAssemblies();

        var flatModules = new List<PEFile>();
        foreach (var projectPeFile in projectPeFiles)
        {
            var assemblyResolver = await _assemblyResolverFactory.GetAssemblyResolver(projectPeFile);
            await CollectPeFiles(projectPeFile, flatModules, assemblyResolver);
        }

        var result = new List<DecompileInfo>();
        foreach (var peFile in flatModules)
        {
            var typeDefinitions = peFile.Metadata.TypeDefinitions;
            foreach (var typeDefinitionHandle in typeDefinitions)
            {
                var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile.Metadata);
                var fullName = fullTypeName.ReflectionName;

                var decompileInfo = new DecompileInfo()
                {
                    AssemblyFilePath = peFile.FileName,
                    AssemblyName = peFile.FullName,
                    Column = 1,
                    ContainingTypeFullName = fullName,
                    EndColumn = 1,
                    Line = 1,
                    SourceText = fullName,
                    StartColumn = 1
                };
                result.Add(decompileInfo);
            }
        }
        
        return result;
    }
}