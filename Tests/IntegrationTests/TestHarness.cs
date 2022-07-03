using System.IO;

namespace IntegrationTests;

static class TestHarness
{
    private static StdIoClient StdIoClient;
    private static string _projectsToTestAgainstRoot;

    public static void Init()
    {
        _projectsToTestAgainstRoot = Path.GetFullPath(@"..\..\..\..\AssembliesToTestAgainst");
        var exePath = "C:\\src\\TryOmnisharpExtension\\StdIoHost\\bin\\Debug\\StdIoHost.exe";
        var targetSolutionPath = "C:\\src\\TryOmnisharpExtension\\AssembliesToTestAgainst\\LibrariesThatReferenceOtherLibraries.sln";
        StdIoClient = new StdIoClient(exePath, targetSolutionPath);
        IoClient.Start();
    }

    public static void Destroy()
    {
        IoClient.Stop();
    }

    public static StdIoClient IoClient => StdIoClient;

    public static string GetLibraryThatReferencesLibraryFilePath(string fileName)
    {
        var result = @$"{_projectsToTestAgainstRoot}\LibraryThatReferencesLibrary\{fileName}";
        return result;
    }
    public static string GetAnotherLibraryThatReferencesLibraryFilePath(string fileName)
    {
        var result = @$"{_projectsToTestAgainstRoot}\AnotherLibraryThatReferencesLibrary\{fileName}";
        return result;
    }

    public static DecompilerClient DecompilerClient()
    {
        return new DecompilerClient();
    }
}