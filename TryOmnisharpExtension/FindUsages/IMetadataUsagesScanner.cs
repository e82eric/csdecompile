using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.FindUsages;

public interface IMetadataUsagesScanner<T>
{
    IEnumerable<ITypeDefinition> GetRootTypesThatUseSymbol(T analyzedSymbol);
}