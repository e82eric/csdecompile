using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    [Export]
    public class IlSpyDecompiledSourceCommandFactory
    {
        private readonly DecompilerFactory _decompilerFactory;

        [ImportingConstructor]
        public IlSpyDecompiledSourceCommandFactory(DecompilerFactory decompilerFactory)
        {
            _decompilerFactory = decompilerFactory;
        }
        
        public async Task<DecompiledSourceResponse> Find(DecompiledSourceRequest request)
        {
            var decompiler = await _decompilerFactory.Get(request.AssemblyFilePath);
            (SyntaxTree syntaxTree, string source) = decompiler.Run(request.ContainingTypeFullName);

            return new DecompiledSourceResponse
            {
                AssemblyFilePath = request.AssemblyFilePath,
                ContainingTypeFullName = request.ContainingTypeFullName,
                IsDecompiled = true,
                SourceText = source
            };
        }
    }
}