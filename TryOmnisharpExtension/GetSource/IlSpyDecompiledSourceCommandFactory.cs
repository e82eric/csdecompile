using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GetSource
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
        
        public DecompiledSourceResponse Find(DecompiledSourceRequest request)
        {
            var symbol = _ilSpySymbolFinder.FindTypeDefinition(
                request.AssemblyFilePath,
                request.ContainingTypeFullName);
            var decompiler = _decompilerFactory.Get(symbol.ParentModule.PEFile.FileName);
            
            (_, string source) = decompiler.Run(symbol);
        
            return new DecompiledSourceResponse
            {
                AssemblyFilePath = request.AssemblyFilePath,
                ContainingTypeFullName = request.ContainingTypeFullName,
                IsDecompiled = true,
                IsFromExternalAssemblies = request.IsFromExternalAssembly,
                SourceText = source,
                Line = request.Line,
                Column = request.Column
            };
        }
    }
}