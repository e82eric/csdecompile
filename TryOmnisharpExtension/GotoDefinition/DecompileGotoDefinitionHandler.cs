using System.Threading.Tasks;
using TryOmnisharpExtension.Roslyn;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class DecompileGotoDefinitionHandler : HandlerBase<DecompiledLocationRequest, DecompileGotoDefinitionResponse>
    {
        private readonly RosylnSymbolInfoFinder<IGotoDefinitionCommand> _rosylnGotoDefinitionCommandFactory;
        private readonly IlSpyCommandFactory<IGotoDefinitionCommand> _ilSpySymbolInfoFinder;

        public DecompileGotoDefinitionHandler(
            RosylnSymbolInfoFinder<IGotoDefinitionCommand> rosylnSymbolInfoFinder,
            IlSpyCommandFactory<IGotoDefinitionCommand> ilSpySymbolInfoFinder)
        {
            _rosylnGotoDefinitionCommandFactory = rosylnSymbolInfoFinder;
            _ilSpySymbolInfoFinder = ilSpySymbolInfoFinder;
        }
        
        public override async Task<DecompileGotoDefinitionResponse> Handle(DecompiledLocationRequest request)
        {
            IGotoDefinitionCommand command;
            if (!request.IsDecompiled)
            {
                command = await _rosylnGotoDefinitionCommandFactory.Get(request);
            }
            else
            {
                command = _ilSpySymbolInfoFinder.Find(request);
            }
            
            var result = await command.Execute();
            return result;
        }
    }

    class FileNotFoundCommand : IGotoDefinitionCommand
    {
        private readonly string _filePath;

        public FileNotFoundCommand(string filePath)
        {
            _filePath = filePath;
        }
        public Task<DecompileGotoDefinitionResponse> Execute()
        {
            var result = new DecompileGotoDefinitionResponse()
            {
                ErrorDetails = new ErrorDetails()
                {
                    Message = $"FILE_NOT_FOUND {_filePath}"
                }
            };
            return Task.FromResult(result);    
        }
    }
    
    class SymbolNotFoundAtLocationCommand : IGotoDefinitionCommand
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
}