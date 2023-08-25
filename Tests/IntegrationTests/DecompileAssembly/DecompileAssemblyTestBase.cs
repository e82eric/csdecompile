using System;
using System.Collections.Generic;
using System.Linq;
using CsDecompileLib;
using CsDecompileLib.GetMembers;
using CsDecompileLib.GetSource;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.Roslyn;
using NUnit.Framework;

namespace IntegrationTests;

public class DecompileAssemblyTestBase : ExternalTestBase
{
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string assemblyFilePath,
        string assemblyName,
        string lineToFind,
        string tokenToRequest,
        string expected)
    {
        var requestArguments = DecompileAssemblyAndCreateRequestForToken(
            assemblyFilePath,
            assemblyName,
            lineToFind,
            tokenToRequest);
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = requestArguments
        };

        RequestAndCompareGotoDefinition(request, expected);
    }
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string endpoint,
        string assemblyFilePath,
        string assemblyName,
        string lineToFind,
        string tokenToRequest,
        IEnumerable<(LocationType type, string value, string shortTypeName)> expected)
    {
        var requestArguments = DecompileAssemblyAndCreateRequestForToken(
            assemblyFilePath,
            assemblyName,
            lineToFind,
            tokenToRequest);
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = endpoint,
            Arguments = requestArguments
        };

        RequestAndCompareFindUsages(request, expected);
    }
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string assemblyFilePath,
        string assemblyName,
        string lineToFind,
        string tokenToRequest,
        Dictionary<string,object> expectedProperties)
    {
        var requestArguments = DecompileAssemblyAndCreateRequestForToken(
            assemblyFilePath,
            assemblyName,
            lineToFind,
            tokenToRequest);
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.SymbolInfo,
            Arguments = requestArguments
        };

        RequestAndCompareGetSymbolInfo(request, expectedProperties);
    }
     
    protected DecompiledLocationRequest DecompileAssemblyAndCreateRequestForToken(
        string assemblyFilePath,
        string assemblyName,
        string lineToFind,
        string tokenToRequest)
    {
        var request = new CommandPacket<DecompileAssemblyRequest>
        {
            Command = Endpoints.DecompileAssembly,
            Arguments = new DecompileAssemblyRequest
            {
                AssemblyFilePath = assemblyFilePath,
                AssemblyName = assemblyName
            }
        };

        var targetClasResponse = TestHarness.IoClient
            .ExecuteCommand<DecompileAssemblyRequest, DecompileAssemlbyResponse>(request);

        Assert.True(targetClasResponse.Success);
        var targetLines = GetLines(targetClasResponse.Body.SourceText);
        var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind));
        var newLine = Array.IndexOf(targetLines, lineText);
        var newColumn = lineText.IndexOf(tokenToRequest);

        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = null,
            AssemblyFilePath = assemblyFilePath,
            Column = newColumn + 1,
            Type = LocationType.DecompiledAssembly,
            Line = newLine + 1
        };
        return decompiledLocationRequest;
    }
    
    private void RequestAndCompareFindUsages(
        CommandPacket<DecompiledLocationRequest> request,
        IEnumerable<(LocationType type, string value, string shortTypeName)> expected)
    {
        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);

        Assert.True(response.Success);

        Assert.AreEqual(expected.Count(), response.Body.Locations.Count());

        foreach (var implementation in response.Body.Locations)
        {
            string[] lines = null;
            switch (implementation.Type)
            {
                case LocationType.Decompiled:
                    lines = ExternalGetLines(implementation);
                    break;
                case LocationType.SourceCode:
                    lines = InSourceGetLines(implementation);
                    break;
            }

            var sourceLine = lines[implementation.Line - 1].Trim();

            var foundExpected = expected.FirstOrDefault(e =>
                e.type == implementation.Type &&
                e.value.Equals(sourceLine));
            Assert.NotNull(foundExpected);
            Assert.AreEqual(foundExpected.type, implementation.Type);
            Assert.AreEqual(implementation.SourceText, sourceLine);
            Assert.AreEqual(foundExpected.shortTypeName, implementation.ContainingTypeShortName);
        }
    }
    
    private static string[] ExternalGetLines(ResponseLocation implementation)
    {
        var decompileInfo = (DecompileInfo)implementation;

        var sourceRequest = new CommandPacket<DecompileInfo>
        {
            Command = Endpoints.DecompiledSource,
            Arguments = new DecompileInfo
            {
                ParentAssemblyFilePath = decompileInfo.ParentAssemblyFilePath,
                AssemblyFilePath = decompileInfo.AssemblyFilePath,
                ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
                Column = decompileInfo.Column,
                Line = decompileInfo.Line
            }
        };

        var sourceResponse = TestHarness.IoClient
            .ExecuteCommand<DecompileInfo, DecompiledSourceResponse>(sourceRequest);

        var lines = GetLines(sourceResponse.Body.SourceText);
        return lines;
    }
    
    private void RequestAndCompareGotoDefinition(CommandPacket<DecompiledLocationRequest> request, string expected)
    {
        var sourceResponse = GotoDefinitionHelper.Run(IoClient, request);
        var lines = GetLines(sourceResponse.Body.SourceText);
        var sourceLine = lines[sourceResponse.Body.Location.Line - 1].Trim();
        Assert.AreEqual(expected, sourceLine);
    }
    
    private void RequestAndCompareGetSymbolInfo(CommandPacket<DecompiledLocationRequest> request,  Dictionary<string,object> expectedProperties)
    {
        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, SymbolInfo>(request);

        Assert.True(response.Success);

        foreach (var key in expectedProperties.Keys)
        {
            var expected = expectedProperties[key];
            var actual = response.Body.Properties[key];
            Assert.AreEqual(expected, actual);
        }
    }
}