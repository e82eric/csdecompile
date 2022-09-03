using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsMethodTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesMethodCaller.cs");
    [Test]
    public void NoParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 50,
            line: 10,
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();",
                    "ExternalFindUsagesMethodCaller"),
                (ResponseLocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();",
                    "ExternalFindUsagesMethodUser"),
                (ResponseLocationType.Decompiled,
                    "Run1(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);",
                    "ExternalFindUsagesMethodUser"),
            });
    }
    [Test]
    public void OneParameters()
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindUsages,
            filePath: FilePath,
            column: 50,
            line: 11,
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(string.Empty);",
                    "ExternalFindUsagesMethodCaller"),
                (ResponseLocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(string.Empty);",
                    "ExternalFindUsagesMethodUser"),
                (ResponseLocationType.Decompiled,
                    "Run2(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);",
                    "ExternalFindUsagesMethodUser"),
            });
    }
    [Test]
    public void TwoParameters()
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindUsages,
            filePath: FilePath,
            column: 50,
            line: 12,
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(String.Empty, String.Empty);",
                    "ExternalFindUsagesMethodCaller"),
                (ResponseLocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(string.Empty, string.Empty);",
                    "ExternalFindUsagesMethodUser"),
                (ResponseLocationType.Decompiled,
                    "Run3(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);",
                    "ExternalFindUsagesMethodUser"),
            });
    }
}