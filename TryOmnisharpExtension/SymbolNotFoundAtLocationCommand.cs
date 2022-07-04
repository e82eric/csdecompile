using System.Threading.Tasks;

namespace TryOmnisharpExtension.GotoDefinition;

class SymbolNotFoundAtLocationCommand : INavigationCommand<DecompileGotoDefinitionResponse>
{
    private readonly string _filePath;
    private readonly int _line;
    private readonly int _column;

    public SymbolNotFoundAtLocationCommand(string filePath, int line, int column)
    {
        _filePath = filePath;
        _line = line;
        _column = column;
    }
    public Task<DecompileGotoDefinitionResponse> Execute()
    {
        var result = new DecompileGotoDefinitionResponse()
        {
            ErrorDetails = new ErrorDetails()
            {
                Message = $"SYMBOL_NOT_FOUND_AT_LOCATION {_filePath}:{_line}:{_column}"
            }
        };
        return Task.FromResult(result);    
    }
}