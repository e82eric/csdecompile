using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

public class ExternalFindUsagesMethodWithGenericInParameterTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesMethodWithGenericInParameterUser.cs");
    [Test]
    public void FindUsages()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 14,
            expected: new []
            {
                new ExpectedImplementation(LocationType.SourceCode,
                    "_t1.TryRun(t1, in t2);",
                    "ExternalFindUsagesMethodWithGenericInParameterUser",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesMethodWithGenericInParameterUser`2"),
                new ExpectedImplementation(LocationType.SourceCode,
                    "_t2.TryRun(\"test\", in o);",
                    "ExternalFindUsagesMethodWithGenericInParameterUser",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesMethodWithGenericInParameterUser`2"),
                new ExpectedImplementation(LocationType.SourceCode,
                    "public bool TryRun(T val, in T2 result)",
                    "ExternalFindUsagesMethodWithGenericInParameterSourceImplementation",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesMethodWithGenericInParameterSourceImplementation`2"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public bool TryRun(T val, in T2 result)",
                    "ExternalFindUsagesMethodWithGenericInParameterExternalImplementation",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodWithGenericInParameterExternalImplementation`2"),
                //ILSpy inlines the temporaries that were passed with "in"
                new ExpectedImplementation(LocationType.Decompiled,
                    "externalFindUsagesMethodWithGenericInParameterTarget.TryRun(default, default(T2));",
                    "ExternalFindUsagesMethodWithGenericInParameterExternalUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodWithGenericInParameterExternalUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "externalFindUsagesMethodWithGenericInParameterTarget2.TryRun(\"test\", new object());",
                    "ExternalFindUsagesMethodWithGenericInParameterExternalUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodWithGenericInParameterExternalUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "return TryRun(val, in result);",
                    "ExternalFindUsagesMethodWithGenericInParameterExternalImplementation",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesMethodWithGenericInParameterExternalImplementation`2")
            });
    }
}