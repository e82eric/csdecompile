using System.Threading.Tasks;

namespace TryOmnisharpExtension.GetSource
{
    public class DecompiledSourceHandler
    {
        private readonly IlSpyDecompiledSourceCommandFactory _commandFactory;

        public DecompiledSourceHandler(IlSpyDecompiledSourceCommandFactory commandFactory)
        {
            _commandFactory =  commandFactory;
        }
        
        public Task<DecompiledSourceResponse> Handle(DecompiledSourceRequest request)
        {
            var response = _commandFactory.Find(request);
            return Task.FromResult(response);
        }
    }
}