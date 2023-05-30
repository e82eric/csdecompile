using NUnit.Framework;
using CsDecompileLib;

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
                (LocationType.SourceCode,
                    "var t = ExternalFindUsagesEnumTarget.Type1;",
                    "ExternalFindUsagesEnumCaller"),
                (LocationType.SourceCode,
                    "var t2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumCaller"),
                (LocationType.SourceCode,
                    "var t3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumCaller"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget = ExternalFindUsagesEnumTarget.Type1;",
                    "ExternalFindUsagesEnumUser"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget = ExternalFindUsagesEnumTarget.Type1;",
                    "ExternalFindUsagesEnumUser"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumUser"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumUser"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumUser"),
                (LocationType.Decompiled,
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
                (LocationType.SourceCode,
                    "var t2 = ExternalFindUsagesEnumTarget.Type2;",
                    "ExternalFindUsagesEnumCaller"),
                (LocationType.Decompiled,
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
                (LocationType.SourceCode,
                    "var t3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumCaller"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumUser"),
            });
    }
    
    [Test]
    public void SearchAsMemberFromWithExternalSource()
    {
        this.SendRequestAndAssertLine(
            filePath: FilePath,
            lineToFind: "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget3 = ExternalFindUsagesEnumTarget.Type3;",
            tokenToFind: "(?<token>Type3);$",
            column: 13,
            line: 12,
            expected: new []
            {
                (LocationType.SourceCode,
                    "var t3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumCaller"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesEnumTarget externalFindUsagesEnumTarget3 = ExternalFindUsagesEnumTarget.Type3;",
                    "ExternalFindUsagesEnumUser"),
            });
    }
}