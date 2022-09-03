using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.FindUsages;

public interface IEntityUsedInTypeFinder<T>
{
    IEnumerable<AstNode> Find(
        SyntaxTree syntaxTree,
        ITypeDefinition typeToSearchEntityHandle,
        T usageToFind);
}