﻿using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Frosting;

[TaskName("Build")]
[IsDependentOn(typeof(ResotoreTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild("../StdIoHost/StdIoHost.csproj", new DotNetBuildSettings
        {
            Configuration = context.MsBuildConfiguration
        });
        context.DotNetBuild("../Tests/IntegrationTests/IntegrationTests.csproj", new DotNetBuildSettings
        {
            Configuration = context.MsBuildConfiguration
        });
        context.DotNetBuild("../AssembliesToTestAgainst/LibrariesWithNoDependencies.sln", new DotNetBuildSettings
        {
            Configuration = context.TestSolutionConfiguration,
        });
        context.DotNetBuild("../AssembliesToTestAgainst/LibrariesWithNoDependenciesCore.sln", new DotNetBuildSettings
        {
            Configuration = context.TestSolutionConfiguration,
        });
        context.DotNetBuild("../AssembliesToTestAgainst/LibrariesThatReferenceOtherLibraries.sln", new DotNetBuildSettings
        {
            Configuration = context.TestSolutionConfiguration
        });
        context.DotNetBuild("../AssembliesToTestAgainst/LibrariesThatReferenceOtherLibrariesCore.sln", new DotNetBuildSettings
        {
            Configuration = context.TestSolutionConfiguration
        });
        base.Run(context);
    }
}