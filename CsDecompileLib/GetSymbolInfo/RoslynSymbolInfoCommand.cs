using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace CsDecompileLib.GotoDefinition;

public class RoslynSymbolInfoCommand : INavigationCommand<Roslyn.SymbolInfo>
{
    private readonly ISymbol _symbol;

    public RoslynSymbolInfoCommand(ISymbol symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<Roslyn.SymbolInfo>> Execute()
    {
        var result = new Roslyn.SymbolInfo();
        result.Properties.Add("FullName", _symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        
        result.HeaderProperties.Add("AssemblyPath", _symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        result.HeaderProperties.Add("Namespace", _symbol.ContainingNamespace.ToDisplayString());
        result.HeaderProperties.Add("DisplayName", _symbol.Name);
        result.HeaderProperties.Add("Kind", _symbol.Kind.ToString());

        if (_symbol is ILocalSymbol localSymbol)
        {
            result.Properties.Add("Type", localSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }
        
        if (_symbol is IFieldSymbol fieldSymbol)
        {
            result.Properties.Add("Type", fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }
        
        if (_symbol is IParameterSymbol parameterSymbol)
        {
            result.Properties.Add("Type", parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }
        
        if (_symbol is IPropertySymbol propertySymbol)
        {
            result.Properties.Add("Type", propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }
        
        if (_symbol is IEventSymbol eventSymbol)
        {
            result.Properties.Add("Type", eventSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }

        if (_symbol is IMethodSymbol methodSymbol)
        {
            result.Properties.Add("ReturnType", methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            var parameters = new Dictionary<string, string>();
            foreach (var parameter in methodSymbol.Parameters)
            {
                parameters.Add(parameter.Name, parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            }
            result.Properties.Add("Parameters", parameters);
        }

        var response = new ResponsePacket<Roslyn.SymbolInfo>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}