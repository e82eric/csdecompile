using System;
using System.Collections.Generic;
using System.Linq;
using CsDecompileLib;
using CsDecompileLib.ExternalAssemblies;
using CsDecompileLib.GetMembers;
using CsDecompileLib.GetSource;
using NUnit.Framework;

namespace IntegrationTests;

public class AddExternalDirectoryTestBase : ExternalTestBase
{
    protected void SearchForTokenInDecompiledTypeAndAssertUsages(
        string assemblyFilePath,
        string typeSearchString,
        string lineToFind,
        string tokenToFind,
        IEnumerable<(LocationType type, string value, string shortTypeName)> expected)
    {
        var addExternalDirectoryRequest = new CommandPacket<AddExternalAssemblyDirectoryRequest>
        {
            Command = Endpoints.AddExternalAssemblyDirectory,
            Arguments = new AddExternalAssemblyDirectoryRequest
            {
                DirectoryFilePath = assemblyFilePath
            }
        };

        var targetClasResponse = TestHarness.IoNoSolutionClient
            .ExecuteCommand<AddExternalAssemblyDirectoryRequest, AddExternalAssemblyDirectoryResponse>(addExternalDirectoryRequest);

        Assert.True(targetClasResponse.Success);

        var searchRequest = new CommandPacket<GetTypesRequest>
        {
            Command = Endpoints.GetTypes,
            Arguments = new GetTypesRequest
            {
                SearchString = typeSearchString
            }
        };

        var searchResponse = TestHarness.IoNoSolutionClient
            .ExecuteCommand<GetTypesRequest, DecompiledFindImplementationsResponse>(searchRequest);
        
        Assert.True(targetClasResponse.Success);
        Assert.AreEqual(1, searchResponse.Body.Locations.Count);

        var foundTypeInfo = searchResponse.Body.Locations.First();
        var decompileTypeRequest = new CommandPacket<DecompileInfo>
        {
            Command = Endpoints.DecompiledSource,
            Arguments = new DecompileInfo
            {
                ParentAssemblyFilePath = foundTypeInfo.ParentAssemblyFilePath,
                AssemblyFilePath = foundTypeInfo.AssemblyFilePath,
                Column = foundTypeInfo.Column,
                Line = foundTypeInfo.Line,
                ContainingTypeFullName = foundTypeInfo.ContainingTypeFullName
            }
        };
        
        var searchTypeSourceResponse = TestHarness.IoNoSolutionClient
            .ExecuteCommand<DecompileInfo, DecompiledSourceResponse>(decompileTypeRequest);
        
        Assert.True(searchTypeSourceResponse.Success);

        var searchTypeSourceLines = GetLines(searchTypeSourceResponse.Body.SourceText);
        var searchTypeFoundLineText = searchTypeSourceLines.FirstOrDefault(l => l.Contains(lineToFind));
        var searchTypeFoundLineNumber = Array.IndexOf(searchTypeSourceLines, searchTypeFoundLineText);
        var searchTypeFoundColumnNumber = searchTypeFoundLineText.IndexOf(tokenToFind);

        var usagesRequest = new CommandPacket<DecompiledLocationRequest>()
        {
            Command = Endpoints.DecompileFindUsages,
            Arguments = new DecompiledLocationRequest
            {
                FileName = null,
                AssemblyFilePath = foundTypeInfo.AssemblyFilePath,
                ContainingTypeFullName = foundTypeInfo.ContainingTypeFullName,
                Column = searchTypeFoundColumnNumber + 1,
                Type = LocationType.Decompiled,
                Line = searchTypeFoundLineNumber + 1
            }
        };
        
        var usagesResponse = TestHarness.IoNoSolutionClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(usagesRequest);
        
        Assert.AreEqual(expected.Count(), usagesResponse.Body.Locations.Count);
        foreach (var implementation in usagesResponse.Body.Locations)
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
                e.shortTypeName == implementation.ContainingTypeFullName &&
                e.type == implementation.Type &&
                e.value.Contains(sourceLine));
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
}