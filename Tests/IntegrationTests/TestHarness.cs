using System.Configuration;
using System.IO;
using NUnit.Framework;

namespace IntegrationTests;

static class TestHarness
{
    private static StdIoClient StdIoClient;
    private static string _projectsToTestAgainstRoot;

    public static void Init()
    {
        var configuration = ConfigurationManager.AppSettings["Configuration"];
        TestContext.Out.WriteLine($"Resolved Configuration: {configuration}");
        
        var rootDir = ConfigurationManager.AppSettings["RootDir"];
        TestContext.Out.WriteLine($"Resolved RootDir: {rootDir}");
        
        _projectsToTestAgainstRoot = Path.GetFullPath($@"{rootDir}\AssembliesToTestAgainst");
        TestContext.Out.WriteLine($"Resolved ProjectsToTestAgainstRootDir: {_projectsToTestAgainstRoot}");
        
        var exePath = @$"{rootDir}\StdIoHost\bin\Debug\StdIoHost.exe";
        TestContext.Out.WriteLine($"Resolved StdIoExePath: {exePath}");
        
        var targetSolutionPath = @$"{rootDir}\AssembliesToTestAgainst\LibrariesThatReferenceOtherLibraries.sln";
        TestContext.Out.WriteLine($"Resolved TargetSolutionPath: {targetSolutionPath}");
        
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