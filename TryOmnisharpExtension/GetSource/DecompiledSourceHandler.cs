using System.Threading.Tasks;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.GetSource
{
    public class DecompiledSourceHandler : IRequestHandler<DecompiledSourceRequest, DecompiledSourceResponse>
    {
        private readonly IlSpyDecompiledSourceCommandFactory _commandFactory;
        private readonly IlSpyDecompiledSourceCommandFactory _externalAssembliesCommandFactory;

        public DecompiledSourceHandler(
            IlSpyDecompiledSourceCommandFactory commandFactory,
            IlSpyDecompiledSourceCommandFactory externalAssembliesCommandFactory)
        {
            _commandFactory =  commandFactory;
            _externalAssembliesCommandFactory = externalAssembliesCommandFactory;
        }
        
        public Task<DecompiledSourceResponse> Handle(DecompiledSourceRequest request)
        {
            DecompiledSourceResponse response;
            if (request.IsFromExternalAssembly)
            {
                response = _externalAssembliesCommandFactory.Find(request);
            }
            else
            {
                response = _commandFactory.Find(request);
            }

            return Task.FromResult(response);
        }
    }
}