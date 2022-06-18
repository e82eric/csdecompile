using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension.IlSpy;

public interface IOmnisharpWorkspace
{
    public IEnumerable<string> GetProjectAssemblyPaths();
    Solution CurrentSolution { get; }
}