using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests
{
    class StdIoClient
    {
        private readonly string _exePath;
        private readonly string _targetSolutionPath;

        private readonly Dictionary<int, ManualResetEvent> _requests = new();
        private readonly Dictionary<int, JObject> _responses = new();
        private int _currentSeq = 1000;
        private Process _process;
        private StreamWriter _writer;
        private ManualResetEvent _startResetEvent;

        public StdIoClient(string exePath, string targetSolutionPath)
        {
            _exePath = exePath;
            _targetSolutionPath = targetSolutionPath;
            _startResetEvent = new ManualResetEvent(false);
        }

        public void Start()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _exePath,
                Arguments = _targetSolutionPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            _process = new Process();
            _process.StartInfo = startInfo;
            _process.OutputDataReceived += ProcessOnOutputDataReceived;
            _process.Start();

            _process.BeginOutputReadLine();

            _writer = _process.StandardInput;

            _startResetEvent.WaitOne(TimeSpan.FromSeconds(30));
        }

        public void Stop()
        {
            _process.Kill();
        }

        public ResponsePacket<TResponse> ExecuteCommand<TArgument, TResponse>(CommandPacket<TArgument> commandPacket)
        {
            _currentSeq++;
            commandPacket.Seq = _currentSeq;

            var mse = new ManualResetEvent(false);
            _requests.Add(commandPacket.Seq, mse);

            var commandString = JsonConvert.SerializeObject(commandPacket);
            TestContext.Out.WriteLine("REQUEST");
            TestContext.Out.WriteLine(commandString);
            TestContext.Out.WriteLine(string.Empty);
            _process.StandardInput.WriteLine(commandString);

            mse.WaitOne(TimeSpan.FromSeconds(30));

            if (_responses.TryGetValue(commandPacket.Seq, out var responseJObject))
            {
                TestContext.Out.WriteLine("RESPONSE");
                TestContext.Out.WriteLine(responseJObject);

                var result = responseJObject.ToObject<ResponsePacket<TResponse>>(new JsonSerializer
                {
                    Converters = { new ResponseLocationConverter() },

                });
                return result;
            }

            return default;
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var jObject = JObject.Parse(e.Data);
            Trace.WriteLine(jObject);
            if (jObject.TryGetValue("Event", out var eventValue))
            {
                Trace.WriteLine(eventValue);
                if (eventValue.Value<string>() == "ASSEMBLIES_LOADED")
                {
                    _startResetEvent.Set();
                }
            }

            if (jObject.TryGetValue("Request_seq", out var seq))
            {
                var prasedSeq = seq.Value<int>();
                if (_requests.TryGetValue(prasedSeq, out var command))
                {
                    _responses.Add(prasedSeq, jObject);
                    command.Set();
                }
            }
        }
    }
}