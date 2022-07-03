using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TryOmnisharpExtension;
using TryOmnisharpExtension.GetSource;
using TryOmnisharpExtension.GotoDefinition;

namespace IntegrationTests;

public class ExternalFindImplementationsBase
{
    protected void SendRequestAndAssertLine(
        string filePath,
        int column,
        int line,
        IEnumerable<string> expected)
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindImplementations,
            filePath,
            column,
            line,
            expected);
    }

    protected void SendRequestAndAssertLine(
        string command,
        string filePath,
        int column,
        int line,
        IEnumerable<string> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                IsDecompiled = false,
                Line = line
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);

        Assert.True(response.Success);

        foreach (var implementation in response.Body.Implementations)
        {
            Assert.AreEqual(ResponseLocationType.Decompiled, implementation.Type);
            var decompileInfo = (DecompileInfo)implementation;
            
            var sourceRequest = new CommandPacket<DecompiledSourceRequest>
            {
                Command = Endpoints.DecompiledSource,
                Arguments = new DecompiledSourceRequest
                {
                    AssemblyFilePath = decompileInfo.AssemblyFilePath,
                    ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
                    Column = column,
                    Line = line
                }
            };

            var sourceResponse = TestHarness.IoClient
                .ExecuteCommand<DecompiledSourceRequest, DecompiledSourceResponse>(sourceRequest);

            string[] stringSeparators = { "\r\n" };
            string[] lines = sourceResponse.Body.SourceText.Split(stringSeparators, StringSplitOptions.None);
            var sourceLine = lines[decompileInfo.Line - 1].Trim();

            var foundExpected = expected.FirstOrDefault(e => e.Contains(sourceLine));
            Assert.NotNull(foundExpected);
            Assert.AreEqual(implementation.SourceText, sourceLine);
        }
    }
    
    protected void SendRequestAndAssertLine(
        string command,
        string filePath,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value)> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                IsDecompiled = false,
                Line = line
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);

        Assert.True(response.Success);
        Assert.AreEqual(expected.Count(), response.Body.Implementations.Count());

        foreach (var implementation in response.Body.Implementations)
        {
            string[] lines = null;
            switch (implementation.Type)
            {
                case ResponseLocationType.Decompiled:
                    lines = ExternalGetLines(implementation);
                    break;
                case ResponseLocationType.SourceCode:
                    lines = InSourceGetLines(implementation);
                    break;
            }
            var sourceLine = lines[implementation.Line - 1].Trim();

            var foundExpected = expected.FirstOrDefault(e =>
                e.type == implementation.Type &&
                e.value.Contains(sourceLine));
            Assert.NotNull(foundExpected);
            Assert.AreEqual(foundExpected.type, implementation.Type);
            Assert.AreEqual(implementation.SourceText, sourceLine);
        }
    }
    
    private static string[] InSourceGetLines(ResponseLocation implementation)
    {
        var sourceFileInfo = (SourceFileInfo)implementation;

        string[] stringSeparators = { "\r\n" };
        var lines = File.ReadAllLines(sourceFileInfo.FileName);
        return lines;
    }

    private static string[] ExternalGetLines(ResponseLocation implementation)
    {
        var decompileInfo = (DecompileInfo)implementation;

        var sourceRequest = new CommandPacket<DecompiledSourceRequest>
        {
            Command = Endpoints.DecompiledSource,
            Arguments = new DecompiledSourceRequest
            {
                AssemblyFilePath = decompileInfo.AssemblyFilePath,
                ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
                Column = decompileInfo.Column,
                Line = decompileInfo.Line
            }
        };

        var sourceResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledSourceRequest, DecompiledSourceResponse>(sourceRequest);

        string[] stringSeparators = { "\r\n" };
        var lines = sourceResponse.Body.SourceText.Split(stringSeparators, StringSplitOptions.None);
        return lines;
    }
    
    protected void SendRequestAndAssertLine(
        string command,
        string filePath,
        string lineToFind,
        string tokenToFind,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value)> expected)
    {
        var definitionRequestArguments = new DecompiledLocationRequest
        {
            FileName = filePath,
            Column = column,
            IsDecompiled = false,
            Line = line
        };

        var definitionResponse = TestHarness.DecompilerClient()
            .GotoDefinition(definitionRequestArguments);
        
        var definitionLines = definitionResponse.Body.SourceText.GetLines();

        var foundLine = definitionLines.FirstOrDefault(l => l.Contains(lineToFind));

        if (foundLine == null)
        {
            TestContext.Out.WriteLine($"Line not found {lineToFind}");
        }
        
        var indexOfLineToFind = Array.IndexOf(definitionLines, foundLine);
        var columnOfToken = foundLine.IndexOf(tokenToFind);

        if (columnOfToken == -1)
        {
            TestContext.Out.WriteLine($"Token not found {tokenToFind}");
        }

        var definitionLocation = definitionResponse.Body.GetLocationAsDecompileInfo();
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                Column = columnOfToken + 1,
                IsDecompiled = true,
                Line = indexOfLineToFind + 1,
                ContainingTypeFullName = definitionLocation.ContainingTypeFullName,
                AssemblyFilePath = definitionLocation.AssemblyFilePath
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);
        
        Assert.True(response.Success);
        Assert.AreEqual(expected.Count(), response.Body.Implementations.Count());

        foreach (var implementation in response.Body.Implementations)
        {
            string[] lines = null;
            switch (implementation.Type)
            {
                case ResponseLocationType.Decompiled:
                    lines = ExternalGetLines(implementation);
                    break;
                case ResponseLocationType.SourceCode:
                    lines = InSourceGetLines(implementation);
                    break;
            }
            
            var sourceLine = lines[implementation.Line - 1].Trim();
            
            var lineFromExpected = expected.FirstOrDefault(e =>
                e.value.Contains(sourceLine));

            if (lineFromExpected == default)
            {
                TestContext.Out.WriteLine($"Line not found {sourceLine}");
            }
            
            Assert.AreEqual(lineFromExpected.type, implementation.Type);
            Assert.AreEqual(implementation.SourceText, sourceLine);
        }
    }
}

public static class ResponseExtensions
{
    public static string[] GetLines(DecompileGotoDefinitionResponse response)
    {
        string[] stringSeparators = { "\r\n" };
        var lines = GetLines(response.SourceText);
        return lines;
    }

    public static string[] GetLines(this string sourceText)
    {
        string[] stringSeparators = { "\r\n" };
        var lines = sourceText.Split(stringSeparators, StringSplitOptions.None);
        return lines;
    }

    public static DecompileInfo GetLocationAsDecompileInfo(this DecompileGotoDefinitionResponse response)
    {
        var defintionLocation = (DecompileInfo)response.Location;
        return defintionLocation;
    }

    public static string GetLineAt(string sourceText, int lineNumber)
    {
        var lines = GetLines(sourceText);
        var line = lines[lineNumber];
        return line;
    }

    public static (int line, int column) GetLineByText(string sourceText, string lineToFind, string tokenToFind)
    {
        var definitionLines = sourceText.GetLines();

        var foundLine = definitionLines.FirstOrDefault(l => l.Contains(lineToFind));

        if (foundLine == null)
        {
            TestContext.Out.WriteLine($"Line not found {lineToFind}");
        }
        
        var indexOfLineToFind = Array.IndexOf(definitionLines, foundLine);
        var columnOfToken = foundLine.IndexOf(tokenToFind);

        if (columnOfToken == -1)
        {
            TestContext.Out.WriteLine($"Token not found {tokenToFind}");
        }

        return (indexOfLineToFind, columnOfToken);
    }
    
    public static void SendRequestAndAssertLine(
        string command,
        string filePath,
        string lineToFind,
        string tokenToFind,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value)> expected)
    {
        var definitionRequestArguments = new DecompiledLocationRequest
        {
            FileName = filePath,
            Column = column,
            IsDecompiled = false,
            Line = line
        };
        
        var definitionResponse = TestHarness.DecompilerClient()
            .GotoDefinition(definitionRequestArguments);

        var defintionLocation = definitionResponse.Body.GetLocationAsDecompileInfo();

        var lineObj = GetLineByText(definitionResponse.Body.SourceText, lineToFind, tokenToFind);
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                Column = lineObj.column + 1,
                IsDecompiled = true,
                Line = lineObj.line + 1,
                ContainingTypeFullName = defintionLocation.ContainingTypeFullName,
                AssemblyFilePath = defintionLocation.AssemblyFilePath
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);
        
        Assert.True(response.Success);
        Assert.AreEqual(expected.Count(), response.Body.Implementations.Count());

        foreach (var implementation in response.Body.Implementations)
        {
            AssertImplementation(implementation, expected);
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
                AssemblyFilePath = decompileInfo.AssemblyFilePath,
                ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
                Column = decompileInfo.Column,
                Line = decompileInfo.Line
            }
        };

        var sourceResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledSourceRequest, DecompiledSourceResponse>(sourceRequest);

        string[] stringSeparators = { "\r\n" };
        var lines = sourceResponse.Body.SourceText.Split(stringSeparators, StringSplitOptions.None);
        return lines;
    }
    
    private static string[] InSourceGetLines(ResponseLocation implementation)
    {
        var sourceFileInfo = (SourceFileInfo)implementation;

        string[] stringSeparators = { "\r\n" };
        var lines = File.ReadAllLines(sourceFileInfo.FileName);
        return lines;
    }
    
    private static void SendRequestAndAssertLine(
        string command,
        string filePath,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value)> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                IsDecompiled = false,
                Line = line
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);

        Assert.True(response.Success);
        Assert.AreEqual(expected.Count(), response.Body.Implementations.Count());

        foreach (var implementation in response.Body.Implementations)
        {
            AssertImplementation(implementation, expected);
        }
    }

    public static void AssertImplementation(
        ResponseLocation implementation,
        IEnumerable<(ResponseLocationType type, string value)> expected)
    {
        string[] lines = null;
        switch (implementation.Type)
        {
            case ResponseLocationType.Decompiled:
                lines = ExternalGetLines(implementation);
                break;
            case ResponseLocationType.SourceCode:
                lines = InSourceGetLines(implementation);
                break;
        }
        
        var sourceLine = lines[implementation.Line - 1].Trim();
        
        var lineFromExpected = expected.FirstOrDefault(e =>
            e.value.Contains(sourceLine));

        if (lineFromExpected == default)
        {
            TestContext.Out.WriteLine($"Line not found {sourceLine}");
        }
        
        Assert.AreEqual(lineFromExpected.type, implementation.Type);
        Assert.AreEqual(implementation.SourceText, sourceLine);
    }
}