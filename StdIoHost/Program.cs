using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace StdIoHost
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //There has to be a better way to load this??
            var customPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"BuildHost-netcore");
            AssemblyLoadContext.Default.LoadFromAssemblyPath($"{customPath}\\Microsoft.Build.Locator.dll");
            
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
            else if (solutionPath == "--process")
            {
                if (int.TryParse(args[1], out var processId))
                {
                    HandlerFactory.InitFromProcess(Console.Out, Console.In, processId).GetAwaiter().GetResult();
                }
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
    
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly string _directoryPath;

        public CustomAssemblyLoadContext(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        public Assembly LoadAssembly(string assemblyName)
        {
            string assemblyPath = Path.Combine(_directoryPath, $"{assemblyName}.dll");
            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}