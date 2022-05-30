using System.Composition;
using System.Threading.Tasks;
using Autofac;
using OmniSharp.Mef;

namespace TryOmnisharpExtension
{
    [OmniSharpHandler(Endpoints.DecompiledSource, Languages.Csharp), Shared]
    public class DecompiledSourceHandler : IRequestHandler<DecompiledSourceRequest, DecompiledSourceResponse>
    {
        private readonly IlSpyDecompiledSourceCommandFactory _commandFactory;
        private readonly IlSpyDecompiledSourceCommandFactory _externalAssembliesCommandFactory;

        [ImportingConstructor]
        public DecompiledSourceHandler(
            IlSpyDecompiledSourceCommandFactory commandFactory,
            ExtensionContainer extensionContainer)
        {
            _commandFactory =  commandFactory;
            _externalAssembliesCommandFactory =
                extensionContainer.Container.Resolve<IlSpyDecompiledSourceCommandFactory>();
        }
        
        public async Task<DecompiledSourceResponse> Handle(DecompiledSourceRequest request)
        {
            DecompiledSourceResponse response;
            if (request.IsFromExternalAssembly)
            {
                response = await _externalAssembliesCommandFactory.Find(request);
            }
            else
            {
                response = await _commandFactory.Find(request);
            }

            return response;
        }
    }
}