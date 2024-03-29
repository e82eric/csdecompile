﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

public class InSourceBase : TestBase
{
    protected void RequestAndAssertCorrectLocations(
        string filePath,
        int column,
        int line,
        IEnumerable<ExpectedImplementation> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                Type = LocationType.SourceCode,
                Line = line
            }
        };
        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);
        Assert.True(response.Success);
        Assert.AreEqual(expected.Count(), response.Body.Locations.Count);

        ImplementationAsserts.AssertSame(response, expected);
    }

    protected void RequestAndAssertCorrectImplementations(
        string filePath,
        int column,
        int line,
        IEnumerable<ExpectedImplementation> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                Type = LocationType.SourceCode,
                Line = line
            }
        };
        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);
        Assert.True(response.Success);
        Assert.AreEqual(expected.Count(), response.Body.Locations.Count);

        ImplementationAsserts.AssertSame2(response, expected);
    }

    protected void RequestAndAssertCorrectLine(
        string filePath,
        int column,
        int line,
        ExpectedImplementation expected)
    {
        RequestAndAssertCorrectLine(
            Endpoints.DecompileGotoDefinition,
            filePath,
            column,
            line,
            expected);
    }
    
    protected void RequestAndAssertCorrectLine(
        string command,
        string filePath,
        int column,
        int line,
        ExpectedImplementation expected)
    {
        var response = ExecuteRequest<LocationsResponse>(command, filePath, column, line);

        AssertInSource(response, expected);
    }

    protected static ResponsePacket<T> ExecuteRequest<T>(string command, string filePath, int column, int line)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                Type = LocationType.SourceCode,
                Line = line
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, T>(request);
        Assert.True(response.Success);
        return response;
    }

    private static void AssertInSource(
        ResponsePacket<LocationsResponse> response,
        ExpectedImplementation expected)
    {
        var implementations = response.Body.Locations;
        Assert.AreEqual(1, implementations.Count);
        var location = implementations.First();
        Assert.AreEqual(location.Type, LocationType.SourceCode);

        ImplementationAsserts.AssertSame(response, new []{ expected});

        var lines = InSourceGetLines(response.Body.Locations.First());
        var line = lines[location.Line - 1].Trim();
        Assert.AreEqual(expected.Line, line);
    }
}