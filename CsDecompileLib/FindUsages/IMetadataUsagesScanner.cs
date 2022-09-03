using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

public interface IMetadataUsagesScanner<T>
{
    IEnumerable<ITypeDefinition> GetRootTypesThatUseSymbol(T analyzedSymbol);
}