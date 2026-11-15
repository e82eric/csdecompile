using System;
using Cake.Frosting;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .InstallTool(new Uri("nuget:?package=NuGet.CommandLine&version=6.14.0"))
            .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.22.0"))
            .Run(args);
    }
}