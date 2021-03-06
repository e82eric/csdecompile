using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using OmniSharp;

namespace TryOmnisharpExtension.IlSpy;

[Export(typeof(IOmnisharpWorkspace))]
public class OmnisharpWorkspaceWrapper : IOmnisharpWorkspace
{
    private readonly OmniSharpWorkspace _omniSharpWorkspace;

    [ImportingConstructor]
    public OmnisharpWorkspaceWrapper(OmniSharpWorkspace omniSharpWorkspace)
    {
        _omniSharpWorkspace = omniSharpWorkspace;
    }

    public IEnumerable<string> GetProjectAssemblyPaths()
    {
        var result = _omniSharpWorkspace.CurrentSolution.Projects.Select(p => p.OutputFilePath).Where(p => p != null).ToList();
        return result;
    }

    public Solution CurrentSolution => _omniSharpWorkspace.CurrentSolution;
}