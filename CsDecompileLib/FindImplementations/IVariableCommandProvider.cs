using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public interface IVariableCommandProvider<TCommandType>
{
    public (bool, TCommandType, ISymbol) GetNodeInformation(DecompiledLocationRequest request);
}