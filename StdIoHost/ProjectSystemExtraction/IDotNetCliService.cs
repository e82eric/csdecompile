using System;
using System.Diagnostics;
using System.Threading.Tasks;

public interface IDotNetCliService
{
    /// <summary>
    /// Launches "dotnet --info" in the given working directory and returns a
    /// <see cref="DotNetInfo"/> representing the returned information text.
    /// </summary>
    DotNetInfo GetInfo(string workingDirectory = null);

    /// <summary>
    /// Launches "dotnet restore" in the given working directory.
    /// </summary>
    /// <param name="workingDirectory">The working directory to launch "dotnet restore" within.</param>
    /// <param name="arguments">Additional arguments to pass to "dotnet restore"</param>
    /// <param name="onFailure">A callback that will be invoked if "dotnet restore" does not
    /// return a success code.</param>
    Task RestoreAsync(string workingDirectory, string arguments = null, Action onFailure = null);
}