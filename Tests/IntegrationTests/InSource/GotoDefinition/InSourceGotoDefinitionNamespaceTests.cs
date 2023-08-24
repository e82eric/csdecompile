using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionNamespaceTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionNamespaceUser.cs");

    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectImplementations(
            filePath: FilePath,
            column: 7,
            line: 1,
            expected: new[]
            {
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass1",
                    "InSourceClass1",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass1"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass2",
                    "InSourceClass2",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass2"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass3",
                    "InSourceClass3",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass3"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class InSourceClass1 { }",
                    "InSourceClass1",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass1"),
                new ExpectedImplementation(LocationType.SourceCode,
                    "public class InSourceClass2 { }",
                    "InSourceClass2",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass2"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class InSourceClass3 { }",
                    "InSourceClass3",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass3")
            });
    }

    [Test]
    public void Test2()
    {
        RequestAndAssertCorrectImplementations(
            filePath: FilePath,
            column: 7,
            line: 2,
            expected: new[]
            {
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass1",
                    "InSourceClass1",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass1"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass2",
                    "InSourceClass2",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass2"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass3",
                    "InSourceClass3",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass3"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class InSourceClass1 { }",
                    "InSourceClass1",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass1"),
                new ExpectedImplementation(LocationType.SourceCode,
                    "public class InSourceClass2 { }",
                    "InSourceClass2",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass2"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class InSourceClass3 { }",
                    "InSourceClass3",
                    "InSourceGotoDefinitionNamespaceTarget.InSourceClass3")
            });
    }

    [Test]
    public void Test3()
    {
        RequestAndAssertCorrectImplementations(
            filePath: FilePath,
            column: 45,
            line: 2,
            expected: new[]
            {
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass1",
                    "SubInSourceClass1",
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass1"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass2",
                    "SubInSourceClass2",
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass2"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass3",
                    "SubInSourceClass3",
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass3"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class SubInSourceClass1 { }",
                    "SubInSourceClass1",
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass1"),
                new ExpectedImplementation(LocationType.SourceCode,
                    "public class SubInSourceClass2 { }",
                    "SubInSourceClass2",
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass2"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class SubInSourceClass3 { }",
                    "SubInSourceClass3",
                    "InSourceGotoDefinitionNamespaceTarget.SubNamespace.SubInSourceClass3")
            });
    }
}