using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CsDecompileLib.IlSpy;

public interface ICsDecompileWorkspace
{
    public IEnumerable<string> GetProjectAssemblyPaths();
    Solution CurrentSolution { get; }
    public Document GetDocument(string fileName);
    IEnumerable<Document> GetDocuments(string filePath);
}