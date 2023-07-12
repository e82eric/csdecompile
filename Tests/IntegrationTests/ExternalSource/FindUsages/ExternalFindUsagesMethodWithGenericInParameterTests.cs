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
                (LocationType.SourceCode,
                    "_t1.TryRun(t1, in t2);",
                    "ExternalFindUsagesMethodWithGenericInParameterUser"),
                (LocationType.SourceCode,
                    "_t2.TryRun(\"test\", in o);",
                    "ExternalFindUsagesMethodWithGenericInParameterUser"),
                (LocationType.SourceCode,
                    "public bool TryRun(T val, in T2 result);",
                    "ExternalFindUsagesMethodWithGenericInParameterSourceImplementation"),
                (LocationType.Decompiled,
                    "public bool TryRun(T val, in T2 result)",
                    "ExternalFindUsagesMethodWithGenericInParameterExternalImplementation"),
                //Not sure why but ilspy seems to add this explicit implementation
                (LocationType.Decompiled,
                    "externalFindUsagesMethodWithGenericInParameterTarget.TryRun(default(T1), in result);",
                    "ExternalFindUsagesMethodWithGenericInParameterExternalUser"),
                (LocationType.Decompiled,
                    "externalFindUsagesMethodWithGenericInParameterTarget2.TryRun(\"test\", in result2);",
                    "ExternalFindUsagesMethodWithGenericInParameterExternalUser"),
                (LocationType.Decompiled,
                    "return TryRun(val, in result);",
                    "ExternalFindUsagesMethodWithGenericInParameterExternalImplementation")
            });
    }
}