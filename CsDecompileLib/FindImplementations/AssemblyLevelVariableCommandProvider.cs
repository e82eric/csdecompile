﻿using CsDecompileLib.IlSpy;
using CsDecompileLib.IlSpy.Ast;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class AssemblyLevelVariableCommandProvider<TCommandType> : IVariableCommandProvider<TCommandType>
{
    private readonly ICommandFactory<TCommandType> _commandCommandFactory;
    private readonly DecompilerFactory _decompilerFactory;
    private readonly MemberNodeInTypeAstFinder _memberNodeInTypeAstFinder;
    private readonly IlSpySymbolFinder _symbolFinder;

    public AssemblyLevelVariableCommandProvider(
        DecompilerFactory decompilerFactory,
        MemberNodeInTypeAstFinder memberNodeInTypeAstFinder,
        IlSpySymbolFinder symbolFinder,
        ICommandFactory<TCommandType> commandCommandFactory)
    {
        _decompilerFactory = decompilerFactory;
        _memberNodeInTypeAstFinder = memberNodeInTypeAstFinder;
        _symbolFinder = symbolFinder;
        _commandCommandFactory = commandCommandFactory;
    }

    public (bool, TCommandType, AstNode) GetNodeInformation(DecompiledLocationRequest request)
    {
        var decompiler = _decompilerFactory.Get(request.AssemblyFilePath);
        var (syntaxTree, _) = decompiler.DecompileWholeModule();
        var node = _symbolFinder.GetNodeAt(syntaxTree, request.Line, request.Column);
        TCommandType commandType = default;
        bool found = false;
        ResolveResult parentResolveResult = node.Parent.GetResolveResult();

        if (parentResolveResult is ILVariableResolveResult variableResolveResult)
        {
            var methodNodeFromAssembly = node.FindParentMemberNode();
            var memberSymbol = methodNodeFromAssembly.GetSymbol() as IMember;
            var containingTypeDefinition = _symbolFinder.FindParentType(methodNodeFromAssembly);
            var (syntaxTree1, sourceText) = decompiler.Run(containingTypeDefinition);
            var typeLevelMethodNode = _memberNodeInTypeAstFinder.Find(memberSymbol, syntaxTree1);
            
            var variableCommand = _commandCommandFactory.GetForVariable(
                variableResolveResult.Variable,
                containingTypeDefinition,
                typeLevelMethodNode,
                sourceText,
                request.AssemblyFilePath);
            
            commandType = variableCommand;
            found = true;
        }

        return (found, commandType, node);
    }
}