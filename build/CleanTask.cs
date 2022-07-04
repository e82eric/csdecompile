using Cake.Common.IO;
using Cake.Frosting;

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        //There has to be a less brittle way to do this
        context.CleanDirectory($"../TryOmnisharpExtension/bin/{context.MsBuildConfiguration}");
        context.CleanDirectory($"../TryOmnisharpExtension/obj/{context.MsBuildConfiguration}");
        context.CleanDirectory($"../StdIoHost/bin/{context.MsBuildConfiguration}");
        context.CleanDirectory($"../StdIoHost/obj/{context.MsBuildConfiguration}");
        context.CleanDirectory($"../Tests/IntegrationTests/bin/{context.MsBuildConfiguration}");
        context.CleanDirectory($"../Tests/IntegrationTests/obj/{context.MsBuildConfiguration}");
        base.Run(context);
    }
}