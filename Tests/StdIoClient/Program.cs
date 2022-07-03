using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StdIoClient
{
    internal class Program
    {
        private static Dictionary<int, ManualResetEvent> _commands = new Dictionary<int, ManualResetEvent>();
        public static void Main(string[] args)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "..\\StdIoHost\\bin\\debug\\StdioHost.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            var process = new Process();
            process.StartInfo = startInfo;
            process.OutputDataReceived += ProcessOnOutputDataReceived;
            process.Start();
            
            process.BeginOutputReadLine();
            
            int seq = 1000;
            
            using (StreamWriter writer = process.StandardInput)
            {
                while (true)
                {
                    // var directory = "C:\\src\\";
                    var directory = "C:\\src\\ilspy-clean\\ILSpy\\bin\\Debug\\net6.0-windows";
                    Console.WriteLine("Press key to send command");
                    Console.ReadLine();
                    var command = new
                    {
                        Command = "/alltypes",
                        Arguments = new
                        {
                            Directory = directory,
                            TypeName = "ASTNode"
                        } ,
                        Seq = seq
                    }; 
                    var commandString = JsonConvert.SerializeObject(command);
                    var mse = new ManualResetEvent(false);
                    _commands.Add(seq, mse);
                    
                    process.StandardInput.WriteLine(commandString);
                    seq++;
                    mse.WaitOne();
                }
            }
        }

        private static void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                string jsonFormatted = JValue.Parse(e.Data).ToString(Formatting.Indented);
                Console.WriteLine(jsonFormatted);
                var jObject = JObject.Parse(e.Data);
                if (jObject.TryGetValue("Request_seq", out var seq))
                {
                    var prasedSeq = seq.Value<int>();
                    if (_commands.TryGetValue(prasedSeq, out var command))
                    {
                        command.Set();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}