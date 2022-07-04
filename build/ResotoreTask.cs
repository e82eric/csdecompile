using Cake.Common.Tools.NuGet;
using Cake.Frosting;

[TaskName("Restore-Nuget-Packages")]
[IsDependentOn(typeof(CleanTask))]
public sealed class ResotoreTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.NuGetRestore("../TryOmnisharpExtension.sln");
        context.NuGetRestore("../Tests/Tests.sln");
        base.Run(context);
    }
}