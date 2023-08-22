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
                new ExpectedImplementation(LocationType.SourceCode,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();",
                    "ExternalFindUsagesMethodCaller",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesMethodCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();",
                    "ExternalFindUsagesMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "Run1(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);",
                    "ExternalFindUsagesMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodUser"),
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
                new ExpectedImplementation(LocationType.SourceCode,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(string.Empty);",
                    "ExternalFindUsagesMethodCaller",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesMethodCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(string.Empty);",
                    "ExternalFindUsagesMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "Run2(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);",
                    "ExternalFindUsagesMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodUser"),
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
                new ExpectedImplementation(LocationType.SourceCode,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(String.Empty, String.Empty);",
                    "ExternalFindUsagesMethodCaller",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesMethodCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod(string.Empty, string.Empty);",
                    "ExternalFindUsagesMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "Run3(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);",
                    "ExternalFindUsagesMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodUser"),
            });
    }
}