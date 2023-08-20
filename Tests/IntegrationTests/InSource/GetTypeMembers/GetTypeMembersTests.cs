using System.IO;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class GetTypeMembersTests : GetMembersTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceGetMembersTarget.cs");
    [Test]
    public void GetSimpleMembers()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:1,
            line:2,
            new []
            {
                //I guess right now we are including the declaration in the response
                ("public InSourceGetMembersTarget()", "InSourceGetMembersTarget"),
                ("public string BasicProperty { get; set; }", "InSourceGetMembersTarget"),
                ("public void BasicMethod()", "InSourceGetMembersTarget"),
                ("public void BasicMethod(string param1)", "InSourceGetMembersTarget"),
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
                //I guess right now we are including the declaration in the response
                ("public InSourceGetMembersTarget()", "InSourceGetMembersTarget"),
                ("public string BasicProperty { get; set; }", "InSourceGetMembersTarget"),
                ("public void BasicMethod()", "InSourceGetMembersTarget"),
                ("public void BasicMethod(string param1)", "InSourceGetMembersTarget"),
            });
    }
}