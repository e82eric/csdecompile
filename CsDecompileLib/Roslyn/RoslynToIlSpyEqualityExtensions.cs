using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace CsDecompileLib.Roslyn;

// Decides whether a Roslyn symbol and an ILSpy symbol are the same member.
//
// Both libraries can produce the documentation comment ID of a member
// (M:System.Console.WriteLine(System.String)), which identifies it by declaring type, name,
// generic arity and parameter types. Metadata tokens can't be used: Roslyn usually reads a
// reference assembly (System.Runtime, System.Console, ...) while ILSpy reads the implementation
// assembly, and the same token points at a different member in each of them.
public static class RoslynToIlSpyEqualityExtensions
{
    public static bool AreSameMethod(IMethodSymbol roslynMethodSymbol, IMethod ilSpySymbol)
    {
        // list.Where(...) is seen by Roslyn as a method without the "this" parameter.
        // The declaration is what exists in metadata.
        return HaveSameId(roslynMethodSymbol?.ReducedFrom ?? roslynMethodSymbol, ilSpySymbol);
    }

    public static bool AreSameProperty(IPropertySymbol roslynPropertySymbol, IProperty ilSpySymbol)
    {
        return HaveSameId(roslynPropertySymbol, ilSpySymbol);
    }

    // Fields and events
    public static bool AreMemberSymbol(ISymbol roslynSymbol, IMember ilSpySymbol)
    {
        return HaveSameId(roslynSymbol, ilSpySymbol);
    }

    private static bool HaveSameId(ISymbol roslynSymbol, IEntity ilSpySymbol)
    {
        if (roslynSymbol == null || ilSpySymbol == null)
        {
            return false;
        }

        // List<int>.Add(int) is List<T>.Add(T) in metadata
        var roslynId = roslynSymbol.OriginalDefinition.GetDocumentationCommentId();
        return roslynId != null && roslynId == IdStringProvider.GetIdString(ilSpySymbol);
    }
}
