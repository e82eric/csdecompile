using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Frosting;

[TaskName("Build")]
[IsDependentOn(typeof(ResotoreTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild("../TryOmnisharpExtension.sln", new DotNetBuildSettings
        {
            Configuration = context.MsBuildConfiguration
        });
        context.DotNetBuild("../AssembliesToTestAgainst/LibrariesWithNoDependencies.sln", new DotNetBuildSettings
        {
            Configuration = context.MsBuildConfiguration
        });
        context.DotNetBuild("../AssembliesToTestAgainst/LibrariesThatReferenceOtherLibraries.sln", new DotNetBuildSettings
        {
            Configuration = context.MsBuildConfiguration
        });
        base.Run(context);
    }
}