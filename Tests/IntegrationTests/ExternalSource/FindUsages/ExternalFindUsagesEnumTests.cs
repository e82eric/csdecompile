using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesEnumTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesEnumCaller.cs");
    [Test]
    public void SearchAsType()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "var t = ExternalFindUsagesEnumTarget.Type1;",
                    "ExternalFindUsagesEnumCaller"),
                (ResponseLocationType.SourceCode,
                    "var t2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumCaller"),
                (ResponseLocationType.SourceCode,
                    "var t3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumCaller"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget = ExternalFindUsagesEnumTarget.Type1;",
                    "ExternalFindUsagesEnumUser"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget = ExternalFindUsagesEnumTarget.Type1;",
                    "ExternalFindUsagesEnumUser"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumUser"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumUser"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumUser"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumUser"),
            });
    }
    
    [Test]
    public void SearchAsMember2()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 50,
            line: 10,
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "var t2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumCaller"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumUser"),
            });
    }
    
    [Test]
    public void SearchAsMember3()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 50,
            line: 11,
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "var t3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumCaller"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumUser"),
            });
    }
}