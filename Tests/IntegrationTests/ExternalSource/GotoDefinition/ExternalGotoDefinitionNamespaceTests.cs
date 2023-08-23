using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionNamespaceTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionNamespaceUser.cs");

    [Test]
    public void Test1()
    {
        SendRequestAndAssertImplementations(
            filePath: FilePath,
            column: 7,
            line: 1,
            new[]
            {
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.Class1",
                    "Class1",
                    "ExternalGotoDefinitionNamespaceTarget.Class1"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.Class2",
                    "Class2",
                    "ExternalGotoDefinitionNamespaceTarget.Class2"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.Class3",
                    "Class3",
                    "ExternalGotoDefinitionNamespaceTarget.Class3"),
            });
    }

    [Test]
    public void Test2()
    {
        SendRequestAndAssertImplementations(
            filePath: FilePath,
            column: 7,
            line: 2,
            new[]
            {
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.Class1",
                    "Class1",
                    "ExternalGotoDefinitionNamespaceTarget.Class1"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.Class2",
                    "Class2",
                    "ExternalGotoDefinitionNamespaceTarget.Class2"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.Class3",
                    "Class3",
                    "ExternalGotoDefinitionNamespaceTarget.Class3"),
            });
    }
    [Test]
    public void Test3()
    {
        SendRequestAndAssertImplementations(
            filePath: FilePath,
            column: 45,
            line: 2,
            new[]
            {
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass1",
                    "SubClass1",
                    "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass1"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass2",
                    "SubClass2",
                    "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass2"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass3",
                    "SubClass3",
                    "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass3")
            });
    }
}