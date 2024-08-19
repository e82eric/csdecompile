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

    public (bool, TCommandType, AstNode) GetNodeInformation(DecompiledLocationRequest request)
    {
        bool found = false;
        TCommandType commandType = default;

        ITypeDefinition containingTypeDefinition = null;
        if (request.ParentAssemblyFilePath != null)
        {
            containingTypeDefinition = _symbolFinder.FindTypeDefinition(
                request.ParentAssemblyFilePath,
                request.ContainingTypeFullName);
        }
        if (containingTypeDefinition == null)
        {
            containingTypeDefinition = _symbolFinder.FindTypeDefinition(
                request.AssemblyFilePath,
                request.ContainingTypeFullName);
        }

        if (containingTypeDefinition == null)
        {
            containingTypeDefinition = _symbolFinder.FindTypeDefinition(
                request.ParentAssemblyFilePath,
                request.ContainingTypeFullName);
        }

        var decompiler = _decompilerFactory.Get(containingTypeDefinition.ParentModule.PEFile.FileName);
        (SyntaxTree syntaxTree, string source) = decompiler.Run(containingTypeDefinition);
        if (source == string.Empty)
        {
            (syntaxTree, source) = decompiler.Run(containingTypeDefinition.MetadataToken);
        }
        
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
        
        return (found, commandType, node);
    }
}