using System;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace StdIoHost
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var cancellation = new CancellationTokenSource();
            var stdOut = new SharedTextWriter(Console.Out);
            var host = new Host(Console.In, stdOut);
            host.Start();
            cancellation.Token.WaitHandle.WaitOne();
        }
    }
}