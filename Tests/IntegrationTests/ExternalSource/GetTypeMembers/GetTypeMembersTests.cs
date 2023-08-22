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
                new ExpectedImplementation(LocationType.Decompiled,
                    "public ExternalGetTypeMembersTarget(string param1)",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public void Method1()",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public string Prop1 { get; set; }",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget")
            });
    }
    [Test]
    public void GetFromOutsideOfClas()
    {
        SendRequestAndAssertLine(
            command: Endpoints.GetTypeMembers,
            filePath: FilePath,
            lineToFind: "namespace LibraryThatJustReferencesFramework",
            tokenToFind: " (?<token>LibraryThatJustReferencesFramework)$",
            line: 9,
            column: 17,
            expected: new[]
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public ExternalGetTypeMembersTarget(string param1)",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public void Method1()",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public string Prop1 { get; set; }",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget")
            });
    }
    [Test]
    public void GetForEmptyLineInClass()
    {
        SendRequestAndAssertLine(
            command: Endpoints.GetTypeMembers,
            filePath: FilePath,
            "^$", //blank line
            "(?<token>^$)", //column 0
            line: 9,
            column: 17,
            expected: new[]
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public ExternalGetTypeMembersTarget(string param1)",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public void Method1()",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public string Prop1 { get; set; }",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget")
            });
    }
    [Test]
    public void GetForGetForStartOfLine()
    {
        SendRequestAndAssertLine(
            command: Endpoints.GetTypeMembers,
            filePath: FilePath,
            "public ExternalGetTypeMembersTarget\\(string param1\\)",
            "$(<token> )",
            line: 9,
            column: 17,
            expected: new[]
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public ExternalGetTypeMembersTarget(string param1)",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public void Method1()",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public string Prop1 { get; set; }",
                    "ExternalGetTypeMembersTarget",
                    "LibraryThatJustReferencesFramework.ExternalGetTypeMembersTarget")
            });
    }
}
