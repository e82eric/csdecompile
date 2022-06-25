using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StdIoHost
{
    internal class Host
    {
        private readonly TextReader _input;
        private readonly SharedTextWriter _output;
        private readonly Router _router;

        public Host(TextReader input, SharedTextWriter output, Router router)
        {
            _input = input;
            _output = output;
            _router = router;
        }

        public void Start()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var line = await _input.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }

                    var ignored = Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            var response = await _router.HandleRequest(line);
                            _output.WriteLine(response);
                        }
                        catch (Exception e)
                        {
                            if (e is AggregateException aggregateEx)
                            {
                                e = aggregateEx.Flatten().InnerException;
                            }

                            _output.WriteLine(new EventPacket()
                            {
                                Event = "error",
                                Body = JsonConvert.ToString(e.ToString(), '"', StringEscapeHandling.Default)
                            });
                        }
                    });
                }
            });
        }
    }
}
