using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetSymbolInfo;

public class UnresolvedMemberSymbolInfoCommand : INavigationCommand<SymbolInfo>
{
    private readonly PEFile _peFile;
    private readonly IMember _symbol;

    public UnresolvedMemberSymbolInfoCommand(PEFile peFile, IMember symbol)
    {
        _peFile = peFile;
        _symbol = symbol;
    }

    public Task<ResponsePacket<SymbolInfo>> Execute()
    {
        var toFill = new SymbolInfo();
        toFill.FillUnknownTypeProperties();
        toFill.AddNameAndKind(_symbol.FullName, _symbol.SymbolKind.ToString());

        foreach (var memberReference in _peFile.Metadata.MemberReferences)
        {
            var member = _peFile.Metadata.GetMemberReference(memberReference);
            if (_peFile.Metadata.StringComparer.Equals(member.Name, _symbol.Name))
            {
                var typeFullName = member.Parent.GetFullTypeName(_peFile.Metadata);
                if (_symbol.FullName.StartsWith(typeFullName.ReflectionName))
                {
                    if (member.Parent.Kind == HandleKind.TypeReference)
                    {
                        var typeReference =
                            _peFile.Metadata.GetTypeReference((TypeReferenceHandle)member.Parent);
                        var assemblyReference = _peFile.AssemblyReferences.FirstOrDefault(ar =>
                            ar.Handle == typeReference.ResolutionScope);
                        toFill.ParentAssemblyFullName = assemblyReference.FullName;
                        break;
                    }
                }
            }
        }

        return Task.FromResult(new ResponsePacket<SymbolInfo>
        {
            Success = true,
            Body = toFill
        });
    }
}