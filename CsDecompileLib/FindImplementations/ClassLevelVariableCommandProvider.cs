using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class ClassLevelVariableCommandProvider<TCommandType> : IVariableCommandProvider<TCommandType>
{
    private readonly IlSpySymbolFinder _symbolFinder;
    private readonly ICommandFactory<TCommandType> _commandCommandFactory;
    private readonly DecompilerFactory _decompilerFactory;

    public ClassLevelVariableCommandProvider(
        IlSpySymbolFinder symbolFinder,
        ICommandFactory<TCommandType> commandCommandFactory,
        DecompilerFactory decompilerFactory)
    {
        _symbolFinder = symbolFinder;
        _commandCommandFactory = commandCommandFactory;
        _decompilerFactory = decompilerFactory;
    }

    public (bool, TCommandType, ISymbol) GetNodeInformation(DecompiledLocationRequest request)
    {
        bool found = false;
        TCommandType commandType = default;

        var containingTypeDefinition = _symbolFinder.FindTypeDefinition(
            request.AssemblyFilePath,
            request.ContainingTypeFullName);

        var decompiler = _decompilerFactory.Get(containingTypeDefinition.ParentModule.PEFile.FileName);
        (SyntaxTree syntaxTree, string source) = decompiler.Run(containingTypeDefinition);
        
        var node = _symbolFinder.GetNodeAt(syntaxTree, request.Line, request.Column);

        var parentResolveResult = node.Parent.GetResolveResult();

        if (parentResolveResult is ILVariableResolveResult variableResolveResult)
        {
            var methodNode = node.FindParentMemberNode();
            commandType = _commandCommandFactory.GetForVariable(
                variableResolveResult.Variable,
                containingTypeDefinition,
                methodNode,
                source,
                request.AssemblyFilePath);
            found = true;
        }
        
        var symbolAtLocation = _symbolFinder.FindSymbolFromNode(node);

        return (found, commandType, symbolAtLocation);
    }
}