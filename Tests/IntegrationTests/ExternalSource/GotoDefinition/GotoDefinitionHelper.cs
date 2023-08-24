using System.Linq;
using CsDecompileLib;
using CsDecompileLib.GetSource;
using NUnit.Framework;

namespace IntegrationTests;

public static class GotoDefinitionHelper
{
    public static ResponsePacket<DecompiledSourceResponse> Run(StdIoClient IoClient, CommandPacket<DecompiledLocationRequest> request)
    {
        var response = IoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);

        Assert.True(response.Success);
        Assert.AreEqual(1, response.Body.Locations.Count);

        var locationToGetSourceFor = response.Body.Locations.First();
        Assert.AreEqual(locationToGetSourceFor.Type, LocationType.Decompiled);
        var decompiledLocationToGetSourceFor = (DecompileInfo)locationToGetSourceFor;

        var sourceInfoRequest = new CommandPacket<DecompiledSourceRequest>
        {
            Command = Endpoints.DecompiledSource,
            Arguments = new DecompiledSourceRequest
            {
                ParentAssemblyFilePath = decompiledLocationToGetSourceFor.ParentAssemblyFilePath,
                AssemblyFilePath = decompiledLocationToGetSourceFor.AssemblyFilePath,
                Column = decompiledLocationToGetSourceFor.Column,
                ContainingTypeFullName = locationToGetSourceFor.ContainingTypeFullName,
                Line = decompiledLocationToGetSourceFor.Line
            }
        };

        var sourceResponse =
            IoClient.ExecuteCommand<DecompiledSourceRequest, DecompiledSourceResponse>(sourceInfoRequest);

        return sourceResponse;
    }
}