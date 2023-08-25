using CsDecompileLib.IlSpy;

namespace CsDecompileLib.GetSource
{
    public class IlSpyDecompiledSourceCommandFactory
    {
        private readonly DecompilerFactory _decompilerFactory;
        private readonly IlSpySymbolFinder _ilSpySymbolFinder;

        public IlSpyDecompiledSourceCommandFactory(
            DecompilerFactory decompilerFactory,
            IlSpySymbolFinder ilSpySymbolFinder)
        {
            _decompilerFactory = decompilerFactory;
            _ilSpySymbolFinder = ilSpySymbolFinder;
        }
        
        public ResponsePacket<DecompiledSourceResponse> Find(DecompileInfo request)
        {
            var symbol = _ilSpySymbolFinder.FindTypeDefinition(
                request.ParentAssemblyFilePath,
                request.ContainingTypeFullName);
            var decompiler = _decompilerFactory.Get(symbol.ParentModule.PEFile.FileName);
            
            (_, string source) = decompiler.Run(symbol);

            var body = new DecompiledSourceResponse
            {
                Location = request,
                SourceText = source
            };

            var response = ResponsePacket.Ok(body);
            
            return response;
        }
    }
}