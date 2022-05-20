using System.Collections.Generic;

namespace TryOmnisharpExtension.IlSpy;

public interface IOmnisharpWorkspace
{
    public IEnumerable<string> GetProjectAssemblyPaths();
}