using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CsDecompileLib;

namespace StdIoHost;

internal class Router
{
    private readonly Dictionary<string, IHandler> _handlers;

    public Router(Dictionary<string, IHandler> handlers)
    {
        _handlers = handlers;
    }
        
    public async Task<ResponsePacket> HandleRequest(string json)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        var request = RequestPacket.Parse(json);
        // if (logger.IsEnabled(LogLevel.Debug))
        // {
        //     LogRequest(json, logger, LogLevel.Debug);
        // }

        ResponsePacket response;
        try
        {
            if (_handlers.TryGetValue(request.Command, out var handler))
            {
                response = await handler.Handle(request.ArgumentsStream);
            }
            else
            {
                response = new ResponsePacket
                {
                    Success = false,
                    Message = $"UNKNOWN_COMMAND {request.Command}",
                };
            }
        }
        catch (Exception e)
        {
            if (e is AggregateException aggregateEx)
            {
                e = aggregateEx.Flatten().InnerException;
            }

            // updating the response object here so that the ResponseStream
            // prints the latest state when being closed
            response = new ResponsePacket
            {
                Success = false,
                Message = JsonConvert.ToString(e.ToString(), '"', StringEscapeHandling.Default)
            };
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
        }
        response.Command = request.Command;
        response.Request_seq = request.Seq;
        return response;
    }
}