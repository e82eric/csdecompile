using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindImplementationsMethodWithGenericInParameterTests : ExternalFindImplementationsBase2
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindImplementationsMethodWithGenericInParameterSourceUser.cs");

    [Test]
    public void GenericTypeParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 7,
            expected: new []
            {
                (LocationType.SourceCode,
                    "public class ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation<T, T2> :",
                    "ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation"),
                (LocationType.Decompiled,
                    "public class ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation<T, T2> : ExternalFindImplementationsMethodWithGenericInParameterTarget<T, T2>",
                    "ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation")
            });
    }

    [Test]
    public void GenericMethodParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 14,
            expected: new []
            {
                (LocationType.SourceCode,
                    "public bool TryRun(T val, in T2 result)",
                    "ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation"),
                (LocationType.Decompiled,
                    "public bool TryRun(T val, in T2 result)",
                    "ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation")
            });
    }
}