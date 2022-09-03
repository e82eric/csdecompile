using System.Threading.Tasks;

namespace CsDecompileLib.GotoDefinition;

class SymbolNotFoundAtLocationCommand<T> : INavigationCommand<T>
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
    public Task<ResponsePacket<T>> Execute()
    {
        var response = new ResponsePacket<T>()
        {
            Message = $"SYMBOL_NOT_FOUND_AT_LOCATION {_filePath}:{_line}:{_column}",
            Success = false
        };
        return Task.FromResult(response);    
    }
}