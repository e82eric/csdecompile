using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace StdIoHost
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (!MSBuildLocator.IsRegistered) //MSBuildLocator.RegisterDefaults(); // ensures correct version is loaded up
            {
                var vs2022 = MSBuildLocator.QueryVisualStudioInstances().Where(x => x.Name.Contains("2022")).First(); // find the correct VS setup. There are namy ways to organise logic here, we'll just assume we want VS2022
                MSBuildLocator.RegisterInstance(vs2022); // register the selected instance
                // var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions); // this ensures library is referenced so the compiler would not try to optimise it away (if dynamically loading assemblies or doing other voodoo that can throw the compiler off) - probably less important than the above but we prefer to follow cargo cult here and leave it be
            }

            var solutionPath = "c:\\src\\DumbFrameworkLoggingLibrary\\DumbFrameworkLoggingLibrary.sln";
            using (var w = MSBuildWorkspace.Create())
            {
                var solution = w.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
            }

            var visualStudioInstances = MSBuildLocator .QueryVisualStudioInstances();
            //select NET5, or whatever by modifying Version.Major 
            var instance = visualStudioInstances.First();
            //register the instance
            MSBuildLocator.RegisterMSBuildPath("C:\\Program Files\\Microsoft Visual Studio\\2022\\Professional\\MSBuild\\Current\\Bin\\");
            // MSBuildLocator.RegisterInstance(instance);	
             
            // var solutionPath = args[0];
            // var solutionPath = "c:\\src\\TryToDecompileFindUsages\\TryToDecompileFindUsages.sln";
            var input = Console.In;
            var output = Console.Out;
            
            // MSBuildLocator.RegisterDefaults();
            
            using (var workspace = MSBuildWorkspace.Create())
            {
                var solution = workspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
                var documentId = solution.GetDocumentIdsWithFilePath(
                    "c:\\src\\TryToDecompileFindUsages\\TryToDecompileFindUsages\\Program.cs").FirstOrDefault();
                var document = solution.GetDocument(documentId);
            }
        }
    }
    
    public class SharedTextWriter : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly Thread _thread;
        private readonly BlockingCollection<object> _queue;
        private readonly CancellationTokenSource _cancel;

        public SharedTextWriter(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _queue = new BlockingCollection<object>();
            _cancel = new CancellationTokenSource();
            _thread = new Thread(ProcessWriteQueue) { IsBackground = true, Name = "ProcessWriteQueue" };
            _thread.Start();
        }

        private void DisposeCore(bool disposing)
        {
            // Finish sending queued output to the writer.
            _cancel.Cancel();
            _thread.Join();
            _cancel.Dispose();
        }

        public void Dispose()
        {
            DisposeCore(true);
            GC.SuppressFinalize(this);
        }

        public void WriteLine(object value)
        {
            _queue.Add(value);
        }

        private void ProcessWriteQueue()
        {
            var token = _cancel.Token;
            try
            {
                while (true)
                {
                    if (_queue.TryTake(out var value, Timeout.Infinite, token))
                    {
                        _writer.WriteLine(value);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken != token)
                    throw;
                // else ignore. Exceptions: OperationCanceledException - The CancellationToken has been canceled.
            }
        }
    }
}