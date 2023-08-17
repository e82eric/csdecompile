using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetSymbolInfo;

public class IlSpyTypeDefinitionSymbolInfoCommand : IlSpySymbolInfoCommandBase, INavigationCommand<SymbolInfo>
{
    private readonly ITypeDefinition _symbol;

    public IlSpyTypeDefinitionSymbolInfoCommand(ITypeDefinition symbol)
    {
        _symbol = symbol;
    }

    public Task<ResponsePacket<SymbolInfo>> Execute()
    {
        var result = new SymbolInfo();
        result.Properties.Add("FullName", _symbol.ReflectionName);
        result.Properties.Add("IsStatic", _symbol.IsStatic.ToString());
        result.Properties.Add("IsSealed", _symbol.IsSealed.ToString());

        if (_symbol.DirectBaseTypes.Any())
        {
            var baseTypes = new List<string>();
            foreach (var baseType in _symbol.DirectBaseTypes)
            {
                baseTypes.Add(baseType.FullName);
            }

            result.Properties.Add("BaseTypes", baseTypes);
        }

        if (_symbol.TypeArguments.Any())
        {
            var typeArguments = new List<string>();
            foreach (var typeArgument in _symbol.TypeArguments)
            {
                typeArguments.Add(typeArgument.FullName);
            }

            result.Properties.Add("TypeArguments", typeArguments);
        }

        if (_symbol.TypeParameters.Any())
        {
            var typeParameters = new List<string>();
            foreach (var typeParameter in _symbol.TypeParameters)
            {
                typeParameters.Add(typeParameter.FullName);
            }

            result.Properties.Add("TypeParameters", typeParameters);
        }

        if (_symbol.DeclaringType != null)
        {
            result.Properties.Add("ParentType", _symbol.DeclaringType.ReflectionName);
        }

        AddIlSpyEntityCommonHeaderProperties(result, _symbol);

        var response = new ResponsePacket<SymbolInfo>
        {
            Body = result,
            Success = true
        };

        return Task.FromResult(response);
    }
}