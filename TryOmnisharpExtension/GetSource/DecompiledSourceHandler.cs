using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;

namespace TryOmnisharpExtension
{
    [OmniSharpHandler(Endpoints.DecompiledSource, Languages.Csharp), Shared]
    public class DecompiledSourceHandler : IRequestHandler<DecompiledSourceRequest, DecompiledSourceResponse>
    {
        private readonly IlSpyDecompiledSourceCommandFactory _commandFactory;

        [ImportingConstructor]
        public DecompiledSourceHandler(IlSpyDecompiledSourceCommandFactory commandFactory)
        {
            _commandFactory =  commandFactory;
        }
        
        public async Task<DecompiledSourceResponse> Handle(DecompiledSourceRequest request)
        {
            var response = await _commandFactory.Find(request);
            return response;
        }
    }
}