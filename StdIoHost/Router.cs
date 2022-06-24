﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TryOmnisharpExtension;
using TryOmnisharpExtension.FindImplementations;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.GetMembers;
using TryOmnisharpExtension.GetSource;
using TryOmnisharpExtension.GotoDefinition;

namespace StdIoHost;

internal class Router
{
    private readonly DecompileGotoDefinitionHandler _goToDefinitionHandler;
    private readonly DecompileFindImplementationsHandler _findImplementationsHandler;
    private readonly DecompiledSourceHandler _getSourceHandler;
    private readonly DecompileFindUsagesHandler _findUsagesHandler;
    private readonly GetTypesHandler _searchTypesHandler;
    private readonly GetTypeMembersHandler _getTypeMembersHandler;

    public Router(
        DecompileGotoDefinitionHandler goToDefinitionHandler,
        DecompileFindImplementationsHandler findImplementationsHandler,
        DecompiledSourceHandler decompiledSourceHandler,
        DecompileFindUsagesHandler findUsagesHandler,
        GetTypesHandler getTypesHandler,
        GetTypeMembersHandler getTypeMembersHandler)
    {
        _goToDefinitionHandler = goToDefinitionHandler;
        _findImplementationsHandler = findImplementationsHandler;
        _getSourceHandler = decompiledSourceHandler;
        _findUsagesHandler = findUsagesHandler;
        _searchTypesHandler = getTypesHandler;
        _getTypeMembersHandler = getTypeMembersHandler;
    }
        
    public async Task<ResponsePacket> HandleRequest(string json)
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
            switch (request.Command)
            {
                case Endpoints.DecompileGotoDefinition:
                    response.Body = await RunGotoDefinition(request);
                    break;
                case Endpoints.DecompileFindImplementations:
                    response.Body = await RunFindImplementations(request);
                    break;
                case Endpoints.DecompiledSource:
                    response.Body = await RunGetSource(request);
                    break;
                case Endpoints.DecompileFindUsages:
                    response.Body = await RunFindUsages(request);
                    break;
                case Endpoints.GetTypes:
                    response.Body = await GetSearchTypesHandler(request);
                    break;
                case Endpoints.GetTypeMembers:
                    response.Body = await GetTypeMembers(request);
                    break;
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
        }

        return response;
    }
        
    private async Task<object> GetSearchTypesHandler(RequestPacket request)
    {
        var arguments = DeserializeRequestObject(request.ArgumentsStream);
        var argObject = arguments.ToObject<GetTypesRequest>();
        var responseBody = await _searchTypesHandler.Handle(argObject);
        return responseBody;
    }
        
    private async Task<object> GetTypeMembers(RequestPacket request)
    {
        var arguments = DeserializeRequestObject(request.ArgumentsStream);
        var argObject = arguments.ToObject<GetTypeMembersRequest>();
        var responseBody = await _getTypeMembersHandler.Handle(argObject);
        return responseBody;
    }
        
    private async Task<object> RunFindUsages(RequestPacket request)
    {
        var arguments = DeserializeRequestObject(request.ArgumentsStream);
        var argObject = arguments.ToObject<DecompileFindUsagesRequest>();
        var responseBody = await _findUsagesHandler.Handle(argObject);
        return responseBody;
    }

    private async Task<object> RunGetSource(RequestPacket request)
    {
        var arguments = DeserializeRequestObject(request.ArgumentsStream);
        var argObject = arguments.ToObject<DecompiledSourceRequest>();
        var gotoDefinitionResult = await _getSourceHandler.Handle(argObject);
        return gotoDefinitionResult;
    }
        
    private async Task<object> RunFindImplementations(RequestPacket request)
    {
        var arguments = DeserializeRequestObject(request.ArgumentsStream);
        var argObject = arguments.ToObject<DecompileFindImplementationsRequest>();
        var gotoDefinitionResult = await _findImplementationsHandler.Handle(argObject);
        return gotoDefinitionResult;
    }

    private async Task<object> RunGotoDefinition(RequestPacket request)
    {
        var arguments = DeserializeRequestObject(request.ArgumentsStream);
        var argObject = arguments.ToObject<DecompileGotoDefinitionRequest>();
        var gotoDefinitionResult = await _goToDefinitionHandler.Handle(argObject);
        return gotoDefinitionResult;
    }

    private JToken DeserializeRequestObject(Stream readStream)
    {
        try
        {
            using (var streamReader = new StreamReader(readStream))
            {
                using (var textReader = new JsonTextReader(streamReader))
                {
                    return JToken.Load(textReader);
                }
            }
        }
        catch
        {
            return new JObject();
        }
    }
}