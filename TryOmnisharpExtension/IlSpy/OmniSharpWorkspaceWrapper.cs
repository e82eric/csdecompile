using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using OmniSharp;

namespace TryOmnisharpExtension.IlSpy;

[Export(typeof(IOmniSharpWorkspace))]
public class OmniSharpWorkspaceWrapper : IOmniSharpWorkspace
{
    private readonly OmniSharpWorkspace _omniSharpWorkspace;

    [ImportingConstructor]
    public OmniSharpWorkspaceWrapper(OmniSharpWorkspace omniSharpWorkspace)
    {
        _omniSharpWorkspace = omniSharpWorkspace;
    }

    public IEnumerable<string> GetProjectAssemblyPaths()
    {
        var result = _omniSharpWorkspace.CurrentSolution.Projects.Select(p => p.OutputFilePath).Where(p => p != null).ToList();
        return result;
    }

    public Solution CurrentSolution => _omniSharpWorkspace.CurrentSolution;
    public Document GetDocument(string fileName)
    {
        return _omniSharpWorkspace.GetDocument(fileName);
    }

    public IEnumerable<Document> GetDocuments(string filePath)
    {
        return _omniSharpWorkspace.GetDocuments(filePath);
    }
}