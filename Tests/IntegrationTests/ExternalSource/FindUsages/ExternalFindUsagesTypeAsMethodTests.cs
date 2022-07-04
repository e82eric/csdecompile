using NUnit.Framework;
using TryOmnisharpExtension;

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
                (ResponseLocationType.SourceCode, "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();"),
                (ResponseLocationType.Decompiled, "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();"),
                (ResponseLocationType.Decompiled, "Run(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);"),
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
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(string.Empty);"),
                (ResponseLocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(String.Empty);"),
                (ResponseLocationType.Decompiled,
                    "Run(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);"),
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
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(String.Empty, String.Empty);"),
                (ResponseLocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(String.Empty, String.Empty);"),
                (ResponseLocationType.Decompiled,
                    "Run(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);"),
            });
    }
}