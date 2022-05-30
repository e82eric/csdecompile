using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TryOmnisharpExtension.GetMembers;
using TryOmnisharpExtension.IlSpy;

namespace StdIoHost
{
    public class Host
    {
        private readonly TextReader _input;
        private readonly SharedTextWriter _output;
        private readonly IDictionary<string, Lazy<EndpointHandler>> _endpointHandlers;

        public Host(TextReader input, SharedTextWriter output)
        {
            _input = input;
            _output = output;

            _endpointHandlers = new Dictionary<string, Lazy<EndpointHandler>>();
            _endpointHandlers.Add("/ericendpoint", new Lazy<EndpointHandler>(() => new TestHandler()));

            var peFileCache = new PeFileCache();
            var assemblyResolverFactory = new AssemblyResolverFactory(peFileCache);
            var allTypesRepository = new IlSpyAllTypesRepository(assemblyResolverFactory);
            var allTypesHandler = new AllTypesHandler(allTypesRepository);
            _endpointHandlers.Add("/alltypes", new Lazy<EndpointHandler>(() => allTypesHandler));
        }

        public void Start()
        {
            Task.Factory.StartNew(async () =>
            {
                _output.WriteLine(new EventPacket()
                {
                    Event = "started"
                });
        
                // while (!_cancellationTokenSource.IsCancellationRequested)
                while(true)
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
                            await HandleRequest(line);
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
                // _logger.LogInformation($"Omnisharp server running using {nameof(TransportType.Stdio)} at location '{_environment.TargetDirectory}' on host {_environment.HostProcessId}.");
    
                // Console.CancelKeyPress += (sender, e) =>
                // {
                //     _cancellationTokenSource.Cancel();
                //     e.Cancel = true;
                // };
    
                // if (_environment.HostProcessId != -1)
                // {
                //     try
                //     {
                //         var hostProcess = Process.GetProcessById(_environment.HostProcessId);
                //         hostProcess.EnableRaisingEvents = true;
                //         hostProcess.OnExit(() => _cancellationTokenSource.Cancel());
                //     }
                //     catch
                //     {
                //         // If the process dies before we get here then request shutdown
                //         // immediately
                //         _cancellationTokenSource.Cancel();
                //     }
                // }
        }
        
        private async Task HandleRequest(string json)
        {
            var startTimestamp = Stopwatch.GetTimestamp();
            var request = RequestPacket.Parse(json);
            // if (logger.IsEnabled(LogLevel.Debug))
            // {
            //     LogRequest(json, logger, LogLevel.Debug);
            // }

            var response = request.Reply();

            try
            {
                if (!request.Command.StartsWith("/"))
                {
                    request.Command = $"/{request.Command}";
                }
                // hand off request to next layer
                if (_endpointHandlers.TryGetValue(request.Command, out var handler))
                {
                    var result = await handler.Value.Handle(request);
                    response.Body = result;
                    return;
                }
                throw new NotSupportedException($"Command '{request.Command}' is not supported.");
            }
            catch (Exception e)
            {
                if (e is AggregateException aggregateEx)
                {
                    e = aggregateEx.Flatten().InnerException;
                }

                // updating the response object here so that the ResponseStream
                // prints the latest state when being closed
                response.Success = false;
                response.Message = JsonConvert.ToString(e.ToString(), '"', StringEscapeHandling.Default);
            }
            finally
            {
                // // response gets logged when Debug or more detailed log level is enabled
                // // or when we have unsuccessful response (exception)
                // if (logger.IsEnabled(LogLevel.Debug) || !response.Success)
                // {
                //     // if logging is at Debug level, request would have already been logged
                //     // however not for higher log levels, so we want to explicitly log the request too
                //     if (!logger.IsEnabled(LogLevel.Debug))
                //     {
                //         LogRequest(json, logger, LogLevel.Warning);
                //     }
                //
                //     var currentTimestamp = Stopwatch.GetTimestamp();
                //     var elapsed = new TimeSpan((long)(TimestampToTicks * (currentTimestamp - startTimestamp)));
                //
                //     LogResponse(response.ToString(), logger, response.Success, elapsed);
                // }
                //
                // // actually write it
                _output.WriteLine(response);
            }
        }
    }
}