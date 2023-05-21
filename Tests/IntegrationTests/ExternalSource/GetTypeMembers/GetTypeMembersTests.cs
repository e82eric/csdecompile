using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests.ExternalSource.GetTypeMembers;

[TestFixture]
public class GetTypeMembersTests : ExternalFindImplementationsBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGetTypeMembersCaller.cs");

    [Test]
    public void Publish()
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
}
