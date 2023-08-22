using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsDecompileLib;
using CsDecompileLib.GetSource;
using NUnit.Framework;

namespace IntegrationTests;

public static class ImplementationAsserts
{
    public static void AssertSame(
        ResponsePacket<FindImplementationsResponse> response,
        IEnumerable<ExpectedImplementation> expected)
    {
        Assert.True(response.Success);

        foreach (var implementation in response.Body.Implementations)
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
                e.ShortName == implementation.ContainingTypeShortName &&
                e.FullName == implementation.ContainingTypeFullName &&
                e.Type == implementation.Type &&
                e.Line.Contains(sourceLine));
            Assert.NotNull(foundExpected);
            Assert.AreEqual(foundExpected.Type, implementation.Type);
            if (foundExpected.Type == LocationType.Decompiled)
            {
                Assert.AreEqual(implementation.SourceText, sourceLine);
            }
            Assert.AreEqual(foundExpected.ShortName, implementation.ContainingTypeShortName);
        }
    }
    private static string[] ExternalGetLines(ResponseLocation implementation)
    {
        var decompileInfo = (DecompileInfo)implementation;

        var sourceRequest = new CommandPacket<DecompiledSourceRequest>
        {
            Command = Endpoints.DecompiledSource,
            Arguments = new DecompiledSourceRequest
            {
                ParentAssemblyFilePath = decompileInfo.ParentAssemblyFilePath,
                AssemblyFilePath = decompileInfo.AssemblyFilePath,
                ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
                Column = decompileInfo.Column,
                Line = decompileInfo.Line
            }
        };

        var sourceResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledSourceRequest, DecompiledSourceResponse>(sourceRequest);

        var lines = GetLines(sourceResponse.Body.SourceText);
        return lines;
    }

    private static string[] InSourceGetLines(ResponseLocation implementation)
    {
        var sourceFileInfo = (SourceFileInfo)implementation;
        var lines = File.ReadAllLines(sourceFileInfo.FileName);
        return lines;
    }

    private static string[] GetLines(string sourceText)
    {
        string[] stringSeparators = { "\r\n" };
        var lines = sourceText.Split(stringSeparators, StringSplitOptions.None);
        return lines;
    }
}