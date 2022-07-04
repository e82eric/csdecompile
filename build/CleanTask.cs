using System;
using Cake.Common.IO;
using Cake.Frosting;

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        try
        {
            //There has to be a less brittle way to do this
            context.DeleteDirectory("../TryOmnisharpExtension/bin", new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });
            context.DeleteDirectory("../TryOmnisharpExtension/obj", new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });
            context.DeleteDirectory("../StdIoHost/bin", new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });
            context.DeleteDirectory("../StdIoHost/obj", new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });
            context.DeleteDirectory("../Tests/IntegrationTests/bin", new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });
            context.DeleteDirectory("../Tests/IntegrationTests/obj", new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });

            }
        catch (Exception e)
        {
        }
        base.Run(context);
    }
}