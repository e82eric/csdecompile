using System.Linq;
using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetSymbolInfo;

public class UnresolvedTypeSymbolInfoCommand : INavigationCommand<SymbolInfo>
{
    private readonly PEFile _peFile;
    private readonly ITypeDefinition _symbol;

    public UnresolvedTypeSymbolInfoCommand(PEFile peFile, ITypeDefinition symbol)
    {
        _peFile = peFile;
        _symbol = symbol;
    }

    public Task<ResponsePacket<SymbolInfo>> Execute()
    {
        var toFill = new SymbolInfo();
        toFill.FillUnknownTypeProperties();
        toFill.AddNameAndKind(_symbol.FullName, _symbol.SymbolKind.ToString());

        foreach (var typeReference in _peFile.Metadata.TypeReferences)
        {
            var typeDef = _peFile.Metadata.GetTypeReference(typeReference);
            var foundTypeName = _peFile.Metadata.GetString(typeDef.Name);
            var nspace = _peFile.Metadata.GetString(typeDef.Namespace);
            if (foundTypeName == _symbol.Name && nspace == _symbol.Namespace)
            {
                var assemblyReference = _peFile.AssemblyReferences.FirstOrDefault(
                    r => r.Handle == typeDef.ResolutionScope);
                toFill.FillFromAssemblyReference(assemblyReference);
                break;
            }
        }

        return Task.FromResult(new ResponsePacket<SymbolInfo>
        {
            Success = true,
            Body = toFill
        });
    }
}