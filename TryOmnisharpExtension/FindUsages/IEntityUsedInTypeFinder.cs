using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

public interface IEntityUsedInTypeFinder<T>
{
    IEnumerable<UsageAsTextLocation> Find(
        (SyntaxTree SyntaxTree, string SourceText) decompiledTypeDefinition,
        ITypeDefinition typeToSearchEntityHandle,
        T usageToFind);
}