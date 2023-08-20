using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class GetInterfaceMembersTests : GetMembersTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceGetInterfaceMembersTarget.cs");
    [Test]
    public void GetSimpleMembers()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:9,
            line:7,
            new []
            {
                ("void Method1();", "InSourceGetInterfaceMembersTarget"),
                ("string Prop1 { get; set; }", "InSourceGetInterfaceMembersTarget"),
                ("IEnumerable<string> Prop2 { get; set; }", "InSourceGetInterfaceMembersTarget"),
            });
    }

    [Test]
    public void GetFromOutsideClass()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:1,
            line:1,
            new []
            {
                ("void Method1();", "InSourceGetInterfaceMembersTarget"),
                ("string Prop1 { get; set; }", "InSourceGetInterfaceMembersTarget"),
                ("IEnumerable<string> Prop2 { get; set; }", "InSourceGetInterfaceMembersTarget"),
            });
    }
}