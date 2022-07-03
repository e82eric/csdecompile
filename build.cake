#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    /* CleanDirectory($"./src/Example/bin/{configuration}"); */
});

Task("Restore-NuGet-Packages")
	.IsDependentOn("Clean")
	.Does(() =>
{
	NuGetRestore("TryOmnisharpExtension.sln");
	NuGetRestore("Tests/Tests.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetBuild("TryOmnisharpExtension.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
    DotNetBuild("AssembliesToTestAgainst/LibrariesWithNoDependencies.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
    DotNetBuild("AssembliesToTestAgainst/LibrariesThatReferenceOtherLibraries.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
    DotNetBuild("Tests/Tests.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./Tests/IntegrationTests/bin/" + configuration + "/IntegrationTests.dll", new NUnit3Settings {
        NoResults = false
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
