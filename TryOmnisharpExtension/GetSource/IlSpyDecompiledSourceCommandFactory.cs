using System.Composition;
using ICSharpCode.Decompiler.CSharp.Syntax;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    [Export]
    public class IlSpyDecompiledSourceCommandFactory
    {
        private readonly DecompilerFactory _decompilerFactory;
        private readonly IlSpySymbolFinder _ilSpySymbolFinder;

        [ImportingConstructor]
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
            var decompiler = _decompilerFactory.Get(request.AssemblyFilePath);
            
            (SyntaxTree syntaxTree, string source) = decompiler.Run(symbol);

            var usage = new UsageAsTextLocation
            {
                StartLocation = new TextLocation(request.Line, request.Column),
                EndLocation = new TextLocation(request.Line, request.Column),
            };
        
            return new DecompiledSourceResponse
            {
                AssemblyFilePath = request.AssemblyFilePath,
                ContainingTypeFullName = request.ContainingTypeFullName,
                IsDecompiled = true,
                IsFromExternalAssemblies = request.IsFromExternalAssembly,
                SourceText = source,
                Line = usage.StartLocation.Line,
                Column = usage.StartLocation.Column
            };
        }
    }
}