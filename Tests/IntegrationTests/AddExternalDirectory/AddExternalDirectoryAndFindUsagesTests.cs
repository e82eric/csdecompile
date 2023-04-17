using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class AddExternalDirectoryAndFindUsagesTests : AddExternalDirectoryTestBase
{
    [Test]
    public void Method()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyBinDir();
        SearchForTokenInDecompiledTypeAndAssertUsages(
            assemblyPath,
            "ExternalFindUsagesMethodOfInterfaceImplementation",
            "public void Run2()",
            "Run2()",
            expected: new []
            {
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface2.Run2();",
                    "ExternalFindUsagesMethodOfInterfaceUser2"),
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface1.Run2();",
                    "ExternalFindUsagesMethodOfInterfaceUser1"),
            });
    }
    
    [Test]
    public void AbstractMethod()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyBinDir();
        SearchForTokenInDecompiledTypeAndAssertUsages(
            assemblyPath,
            "ExternalFindUsagesMethodOfAbstractClassImplementation",
            "public override void Run()",
            "Run()",
            expected: new []
            {
                (LocationType.Decompiled,
                    "_abstractClass.Run();",
                    "ExternalFindUsagesMethodOfAbstractClassUser1"),
                (LocationType.Decompiled,
                    "_abstractClass.Run();",
                    "ExternalFindUsagesMethodOfAbstractClassUser2")
            });
    }
    
    [Test]
    public void GenericMethodStringTypeParameter()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyBinDir();
        SearchForTokenInDecompiledTypeAndAssertUsages(
            assemblyPath,
            "ExternalFindUsagesMethodOfInterfaceImplementation",
            "public void Run2<T>(string p1)",
            "Run2",
            expected: new []
            {
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface2.Run2<string>(\"Test\");",
                    "ExternalFindUsagesMethodOfInterfaceUser2"),
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface1.Run2<string>(\"Test\");",
                    "ExternalFindUsagesMethodOfInterfaceUser1"),
            });
    }
    
    [Test]
    public void GenericMethodNoParameters()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyBinDir();
        SearchForTokenInDecompiledTypeAndAssertUsages(
            assemblyPath,
            "ExternalFindUsagesMethodOfInterfaceImplementation",
            "public void Run2<T>()",
            "Run2",
            expected: new []
            {
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface2.Run2<string>();",
                    "ExternalFindUsagesMethodOfInterfaceUser2"),
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface1.Run2<string>();",
                    "ExternalFindUsagesMethodOfInterfaceUser1"),
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface2.Run2<bool>();",
                    "ExternalFindUsagesMethodOfInterfaceUser2"),
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface1.Run2<bool>();",
                    "ExternalFindUsagesMethodOfInterfaceUser1"),
            });
    }
    
    [Test]
    public void ExplicitInterfaceMethod()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyBinDir();
        SearchForTokenInDecompiledTypeAndAssertUsages(
            assemblyPath,
            "ExternalFindUsagesMethodOfInterfaceImplementation",
            "void ExternalFindUsagesMethodOfInterfaceInterface.Run()",
            "Run()",
            expected: new []
            {
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface2.Run();",
                    "ExternalFindUsagesMethodOfInterfaceUser2"),
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface1.Run();",
                    "ExternalFindUsagesMethodOfInterfaceUser1"),
            });
    }
    
    [Test]
    public void Property()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyBinDir();
        SearchForTokenInDecompiledTypeAndAssertUsages(
            assemblyPath,
            "ExternalFindUsagesPropertyOfInterfaceImplementation",
            "public string Prop1 { get; set; }",
            "Prop1",
            expected: new []
            {
                (LocationType.Decompiled,
                    "string prop = _externalFindUsagesPropertyOfInterfaceInterface2.Prop1;",
                    "ExternalFindUsagesPropertyOfInterfaceUser2"),
                (LocationType.Decompiled,
                    "string prop = _externalFindUsagesPropertyOfInterfaceInterface1.Prop1;",
                    "ExternalFindUsagesPropertyOfInterfaceUser1"),
            });
    }
    
    [Test]
    public void Event()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyBinDir();
        SearchForTokenInDecompiledTypeAndAssertUsages(
            assemblyPath,
            "ExternalFindUsagesEventOfInterfaceImplementation",
            "public event EventHandler Event1;",
            "Event1",
            expected: new []
            {
                (LocationType.Decompiled,
                    "_externalFindUsagesEventOfInterfaceInterface1.Event1 += ExternalFindUsagesEventOfInterfaceInterface1OnEvent1;",
                    "ExternalFindUsagesEventOfInterfaceUser1"),
                (LocationType.Decompiled,
                    "_externalFindUsagesEventOfInterfaceInterface2.Event1 += ExternalFindUsagesEventOfInterfaceInterface2OnEvent2;",
                    "ExternalFindUsagesEventOfInterfaceUser2"),
            });
    }
}