using System;
using System.Linq;
using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests.ExternalSource.GetTypeMembers;

[TestFixture]
public class GetTypeMembersTests : ExternalFindImplementationsBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGetTypeMembersCaller.cs");

    [Test]
    public void GetWithinClass()
    {
        SendRequestAndAssertLine(
            command: Endpoints.GetTypeMembers,
            filePath: FilePath,
            lineToFind: "public class ExternalGetTypeMembersTarget",
            tokenToFind: "ExternalGetTypeMembersTarget",
            line: 9,
            column: 17,
            expected: new[]
            {
                (LocationType.Decompiled,
                    "public ExternalGetTypeMembersTarget(string param1)",
                    "ExternalGetTypeMembersTarget"),
                (LocationType.Decompiled,
                    "public void Method1()",
                    "ExternalGetTypeMembersTarget"),
                (LocationType.Decompiled,
                    "public string Prop1 { get; set; }",
                    "ExternalGetTypeMembersTarget")
            });
    }
    [Test]
    public void GetFromOutsideOfClas()
    {
        SendRequestAndAssertLine(
            command: Endpoints.GetTypeMembers,
            filePath: FilePath,
            lineToFind: "namespace LibraryThatJustReferencesFramework",
            tokenToFind: "ExternalGetTypeMembersTarget",
            line: 9,
            column: 17,
            expected: new[]
            {
                (LocationType.Decompiled,
                    "public ExternalGetTypeMembersTarget(string param1)",
                    "ExternalGetTypeMembersTarget"),
                (LocationType.Decompiled,
                    "public void Method1()",
                    "ExternalGetTypeMembersTarget"),
                (LocationType.Decompiled,
                    "public string Prop1 { get; set; }",
                    "ExternalGetTypeMembersTarget")
            });
    }
    [Test]
    public void GetForEmptyLineInClass()
    {
        SendRequestAndAssertLine(
            command: Endpoints.GetTypeMembers,
            filePath: FilePath,
            line: 9,
            column: 17,
            findInDecompiledSource:(decompiledSourcelines) =>
            {
                var lineText = decompiledSourcelines.FirstOrDefault(l => l.Length == 0);
                var newLine = Array.IndexOf(decompiledSourcelines, lineText) + 1;
                return (newLine, 0);
            },
            expected: new[]
            {
                (LocationType.Decompiled,
                    "public ExternalGetTypeMembersTarget(string param1)",
                    "ExternalGetTypeMembersTarget"),
                (LocationType.Decompiled,
                    "public void Method1()",
                    "ExternalGetTypeMembersTarget"),
                (LocationType.Decompiled,
                    "public string Prop1 { get; set; }",
                    "ExternalGetTypeMembersTarget")
            });
    }
    [Test]
    public void GetForGetForStartOfLine()
    {
        SendRequestAndAssertLine(
            command: Endpoints.GetTypeMembers,
            filePath: FilePath,
            line: 9,
            column: 17,
            findInDecompiledSource:(decompiledSourceLines) =>
            {
                var lineText = decompiledSourceLines.FirstOrDefault(l => l.Contains("public ExternalGetTypeMembersTarget(string param1)"));
                var newLine = Array.IndexOf(decompiledSourceLines, lineText) + 1;
                return (newLine, 1);
            },
            expected: new[]
            {
                (LocationType.Decompiled,
                    "public ExternalGetTypeMembersTarget(string param1)",
                    "ExternalGetTypeMembersTarget"),
                (LocationType.Decompiled,
                    "public void Method1()",
                    "ExternalGetTypeMembersTarget"),
                (LocationType.Decompiled,
                    "public string Prop1 { get; set; }",
                    "ExternalGetTypeMembersTarget")
            });
    }
}
