using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    [Export]
    public class IlSpyDecompiledSourceCommandFactory
    {
        private readonly DecompilerFactory _decompilerFactory;
        private readonly TypeUsedInTypeFinder _typeUsedInTypeFinder;
        private readonly IlSpySymbolFinder _ilSpySymbolFinder;

        [ImportingConstructor]
        public IlSpyDecompiledSourceCommandFactory(
            DecompilerFactory decompilerFactory,
            TypeUsedInTypeFinder typeUsedInTypeFinder,
            IlSpySymbolFinder ilSpySymbolFinder)
        {
            _decompilerFactory = decompilerFactory;
            _typeUsedInTypeFinder = typeUsedInTypeFinder;
            _ilSpySymbolFinder = ilSpySymbolFinder;
        }
        
        public async Task<DecompiledSourceResponse> Find(DecompiledSourceRequest request)
        {
            var symbol = await _ilSpySymbolFinder.FindTypeDefinition(
                request.AssemblyFilePath,
                request.ContainingTypeFullName);
            var decompiler = await _decompilerFactory.Get(request.AssemblyFilePath);
            
            (SyntaxTree syntaxTree, string source) = decompiler.Run(symbol);

            UsageAsTextLocation usage;
            if (request.UsageType == UsageTypes.InMethodBody)
            {
                usage = new UsageAsTextLocation
                {
                    StartLocation = new TextLocation(request.Line, request.Column),
                    EndLocation = new TextLocation(request.Line, request.Column),
                };
            }
            else if (request.UsageType == UsageTypes.MethodImplementation)
            {
                usage = await _typeUsedInTypeFinder.FindMethod(
                    symbol,
                    request.NamespaceName,
                    request.TypeName,
                    request.MethodName);
            }
            else if (request.UsageType == UsageTypes.PropertyImplementation)
            {
                usage = await _typeUsedInTypeFinder.FindProperty(
                    symbol,
                    request.NamespaceName,
                    request.TypeName,
                    request.MethodName);
            }
            else if (request.UsageType == UsageTypes.EventImplementation)
            {
                usage = await _typeUsedInTypeFinder.FindEvent(
                    symbol,
                    request.NamespaceName,
                    request.TypeName,
                    request.MethodName);
            }
            else if (request.UsageType == UsageTypes.Type)
            {
                usage = await _typeUsedInTypeFinder.FindType(
                    request.NamespaceName,
                    symbol.Name,
                    syntaxTree);
            }
            else
            {
                usage = await _typeUsedInTypeFinder.FindType(
                    symbol,
                    request.NamespaceName,
                    request.TypeName,
                    request.BaseTypeName);
            }
        
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