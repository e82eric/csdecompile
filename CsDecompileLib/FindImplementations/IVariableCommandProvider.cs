using ICSharpCode.Decompiler.CSharp.Syntax;

namespace CsDecompileLib.FindImplementations;

public interface IVariableCommandProvider<TCommandType>
{
    public (bool, TCommandType, AstNode) GetNodeInformation(DecompiledLocationRequest request);
}