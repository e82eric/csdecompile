using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindImplementationsMethodWithGenericInParameterTests : ExternalFindImplementationsBase
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
                new ExpectedImplementation(LocationType.SourceCode,
                    "public class ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation<T, T2> :",
                    "ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation",
                    "LibraryThatReferencesLibrary.ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation`2"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public class ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation<T, T2> : ExternalFindImplementationsMethodWithGenericInParameterTarget<T, T2>",
                    "ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation`2")
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
                new ExpectedImplementation(LocationType.SourceCode,
                    "public bool TryRun(T val, in T2 result)",
                    "ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation",
                    "LibraryThatReferencesLibrary.ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation`2"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public bool TryRun(T val, in T2 result)",
                    "ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation`2")
            });
    }
}