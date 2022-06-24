using System;
using System.Threading;
using StdIoHost.ProjectSystemExtraction;

namespace StdIoHost
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var solutionPath = args[0];
            var cancellation = new CancellationTokenSource();
            OmniSharpApplication.Init(Console.Out, Console.In, solutionPath).GetAwaiter().GetResult();
            var host = OmniSharpApplication.CreateHost();
            host.Start();
            cancellation.Token.WaitHandle.WaitOne();
        }
    }
}