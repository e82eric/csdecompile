using System.Collections.Generic;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.Metadata;

namespace CsDecompileLib.GetMembers;

public class AllTypesRepository
{
    private readonly IDecompileWorkspace _workspace;

    public AllTypesRepository(IDecompileWorkspace workspace)
    {
        _workspace = workspace;
    }

    public IEnumerable<DecompileInfo> GetAssemblyType(string assemblyFilePath)
    {
        var peFile = _workspace.GetAssembly(assemblyFilePath);
        var result = GetAssemblyTypes(peFile);
        return result;
    }

    private IList<DecompileInfo> GetAssemblyTypes(PEFile peFile)
    {
        var result = new List<DecompileInfo>();
        var typeDefinitions = peFile.Metadata.TypeDefinitions;
        foreach (var typeDefinitionHandle in typeDefinitions)
        {
            var decompileInfo = PEMetadataHelper.MapTypeDefinitionToDecompileInfo(typeDefinitionHandle, peFile);
            if (decompileInfo != null)
            {
                result.Add(decompileInfo);
            }
        }

        return result;
    }
}