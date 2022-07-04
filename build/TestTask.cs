using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.NUnit;
using Cake.Common.Xml;
using Cake.Frosting;

[TaskName("Test")]
[IsDependentOn(typeof(BuildTask))]
public sealed class TestTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild("../Tests/Tests.sln", new DotNetBuildSettings
        {
            Configuration = context.MsBuildConfiguration
        });

        var rootDir = context.MakeAbsolute(context.Directory("../"));
        
        var configFile = context.File(@$"../Tests/IntegrationTests/bin/{context.MsBuildConfiguration}/IntegrationTests.dll.config");
        context.XmlPoke(configFile, "/configuration/appSettings/add[@key = 'Configuration']/@value", context.MsBuildConfiguration);
        context.XmlPoke(configFile, "/configuration/appSettings/add[@key = 'RootDir']/@value", rootDir.FullPath);
        context.NUnit3($"../Tests/IntegrationTests/bin/{context.MsBuildConfiguration}/IntegrationTests.dll",
            new NUnit3Settings
            {
                NoResults = true,
                TraceLevel = null
            });
    }
}