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
        SendRequestAndAssertLocations(
            filePath: FilePath,
            column: 7,
            line: 1,
            new[]
            {
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.Class1", "Class1"),
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.Class2", "Class2"),
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.Class3", "Class3"),
            });
    }

    [Test]
    public void Test2()
    {
        SendRequestAndAssertLocations(
            filePath: FilePath,
            column: 7,
            line: 2,
            new[]
            {
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.Class1", "Class1"),
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.Class2", "Class2"),
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.Class3", "Class3"),
            });
    }
    [Test]
    public void Test3()
    {
        SendRequestAndAssertLocations(
            filePath: FilePath,
            column: 45,
            line: 2,
            new[]
            {
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass1", "SubClass1"),
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass2", "SubClass2"),
                (LocationType.Decompiled, "ExternalGotoDefinitionNamespaceTarget.SubNamespace.SubClass3", "SubClass3"),
            });
    }
}