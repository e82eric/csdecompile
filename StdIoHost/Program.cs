using System;
using System.Threading;

namespace StdIoHost
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("Platform", "");
            var solutionPath = args[0];
            var cancellation = new CancellationTokenSource();
            if (solutionPath?.ToLower() == "--nosolution")
            {
                HandlerFactory.InitNoSolution(Console.Out, Console.In).GetAwaiter().GetResult();
            }
            else if (solutionPath == "--memorydump")
            {
                HandlerFactory.InitFromMemoryDump(Console.Out, Console.In, args[1]).GetAwaiter().GetResult();
            }
            else
            {
                HandlerFactory.Init(Console.Out, Console.In, solutionPath).GetAwaiter().GetResult();
            }
            var host = HandlerFactory.CreateHost();
            host.Start();
            cancellation.Token.WaitHandle.WaitOne();
        }
    }
}