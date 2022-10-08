namespace CsDecompileLib;

public interface INavigationCommandFactory<TCommandType>
{
    public TCommandType Find(DecompiledLocationRequest request);
}