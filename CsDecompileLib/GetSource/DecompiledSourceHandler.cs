using System.Threading.Tasks;

namespace CsDecompileLib.GetSource
{
    public class DecompiledSourceHandler : HandlerBase<DecompileInfo, DecompiledSourceResponse>
    {
        private readonly IlSpyDecompiledSourceCommandFactory _commandFactory;

        public DecompiledSourceHandler(IlSpyDecompiledSourceCommandFactory commandFactory)
        {
            _commandFactory =  commandFactory;
        }
        
        public override Task<ResponsePacket<DecompiledSourceResponse>> Handle(DecompileInfo request)
        {
            var response = _commandFactory.Find(request);
            return Task.FromResult(response);
        }
    }
}