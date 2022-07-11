using Cake.Common;
using Cake.Core;
using Cake.Frosting;

public class BuildContext : FrostingContext
{
    public string MsBuildConfiguration { get; set; }
    public string TestSolutionConfiguration { get; set; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        MsBuildConfiguration = context.Argument("configuration", "Release");
        TestSolutionConfiguration = "Debug";
    }
}