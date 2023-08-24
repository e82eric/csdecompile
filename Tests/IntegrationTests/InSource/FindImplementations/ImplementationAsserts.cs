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
            var foundExpected = AssertExpectedInImplementations(expected, implementation);

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

            string errorMessage = "";
            var sourceContainsLine = foundExpected.Line.Contains(sourceLine);
            if (sourceContainsLine)
            {
                errorMessage = "Expected line not in source\r\n" +
                               $"\texpected line: {foundExpected.Line}\r\n" +
                               $"\tsource line: {sourceLine}\r\n";
            }
            Assert.True(sourceContainsLine, errorMessage);
            Assert.AreEqual(implementation.SourceText, sourceLine);
        }
    }

    public static void AssertSame2(
        ResponsePacket<FindImplementationsResponse> response,
        IEnumerable<ExpectedImplementation> expected)
    {
        Assert.True(response.Success);

        foreach (var implementation in response.Body.Implementations)
        {
            AssertExpectedInImplementations(expected, implementation);
        }
    }

    private static ExpectedImplementation AssertExpectedInImplementations(IEnumerable<ExpectedImplementation> expected, ResponseLocation implementation)
    {
        var foundExpected = expected.FirstOrDefault(e =>
            e.ShortName == implementation.ContainingTypeShortName &&
            e.FullName == implementation.ContainingTypeFullName &&
            e.Type == implementation.Type &&
            e.Line == implementation.SourceText);

        string errorMessage = "";
        if (foundExpected == null)
        {
            int numberOfShortNameMatches =
                expected.Count(e => e.ShortName == implementation.ContainingTypeShortName);
            int numberOfFullNameMatches = expected.Count(e => e.FullName == implementation.ContainingTypeFullName);
            int numberOfTypeMatches = expected.Count(e => e.Type == implementation.Type);
            int numberOfLineMatches = expected.Count(e => e.Line == implementation.SourceText);
            errorMessage =
                $"Could not find implementation in expected\r\n" +
                $"\tShortName: {implementation.ContainingTypeShortName} \r\n" +
                $"\tFullName: {implementation.ContainingTypeFullName} \r\n" +
                $"\tType: {implementation.Type} \r\n" +
                $"\tLine: {implementation.SourceText} \r\n" +
                $"\tShortName matches: {numberOfShortNameMatches} \r\n" +
                $"\tFullName matches: {numberOfFullNameMatches} \r\n" +
                $"\tType matches: {numberOfTypeMatches} \r\n" +
                $"\tLine matches: {numberOfLineMatches} \r\n";
        }

        Assert.NotNull(foundExpected, errorMessage);
        Assert.AreEqual(foundExpected.Type, implementation.Type);
        Assert.AreEqual(foundExpected.ShortName, implementation.ContainingTypeShortName);
        return foundExpected;
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