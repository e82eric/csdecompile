using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetMembers;

public interface ITypeMembersFinder
{
    IEnumerable<AstNode> Find(
        SyntaxTree syntaxTree,
        ITypeDefinition typeToSearchEntityHandle);
}