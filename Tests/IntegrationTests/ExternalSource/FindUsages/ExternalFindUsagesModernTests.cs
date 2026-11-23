using System;
using System.IO;
using System.Linq;
using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

// Find usages from a net10.0 project of symbols in a compiled library (ModernLibrary) that are also
// used by a second compiled library (ModernLibraryUser). The starting position is found by searching
// the source file for a piece of text.
[TestFixture]
public class ExternalFindUsagesModernTests
{
    private const string CompiledUser = "ModernLibraryUser.ModernNestedUser";

    private static readonly string FilePath =
        TestHarness.GetLibraryThatReferencesModernLibraryFilePath("ModernExternalUser.cs");

    [TestCase("ModernOuter.Inner()", 12, "ModernOuter.Inner inner = new ModernOuter.Inner();")]
    [TestCase("InnerMethod()", 0, "inner.InnerMethod();")]
    [TestCase("inner.InnerField", 6, "inner.InnerField = 1;")]
    [TestCase("inner.InnerProp", 6, "inner.InnerProp = 2;")]
    [TestCase("Inner.Deepest()", 6, "ModernOuter.Inner.Deepest deepest = new ModernOuter.Inner.Deepest();")]
    [TestCase("DeepestMethod()", 0, "deepest.DeepestMethod();")]
    public void NestedTypeUsedByAnotherCompiledAssembly(string textToFind, int offset, string expectedLine)
    {
        var response = FindUsages(textToFind, offset);

        var inCompiledUser = response.Locations
            .Where(l => l.Type == LocationType.Decompiled && l.ContainingTypeFullName == CompiledUser)
            .Select(l => l.SourceText.Trim())
            .ToList();
        CollectionAssert.Contains(inCompiledUser, expectedLine);
    }

    // Goes to the definition first, then asks for the usages of the declaration it landed on in the
    // decompiled code. The usages in the source projects have to be part of the answer.
    [TestCase("ModernOuter.Inner()", 12, "var inner = new ModernOuter.Inner();")]
    [TestCase("InnerMethod()", 0, "inner.InnerMethod();")]
    [TestCase("InnerEvent +=", 0, "inner.InnerEvent += (s, e) => { };")]
    [TestCase("inner.InnerField", 6, "var innerField = inner.InnerField;")]
    [TestCase("inner.InnerProp", 6, "var innerProp = inner.InnerProp;")]
    [TestCase("Inner.Deepest()", 6, "var deepest = new ModernOuter.Inner.Deepest();")]
    [TestCase("DeepestMethod()", 0, "deepest.DeepestMethod();")]
    [TestCase("ModernGeneric<int>()", 0, "var generic = new ModernGeneric<int>();")]
    [TestCase("GenEvent +=", 0, "generic.GenEvent += (s, e) => { };")]
    [TestCase("Take(1)", 0, "generic.Take(1);")]
    [TestCase("generic.GenProp", 8, "var genericProp = generic.GenProp;")]
    public void FromDecompiledDeclarationIncludesSourceUsages(string textToFind, int offset, string expectedLine)
    {
        var definition = (DecompileInfo)Send(Endpoints.DecompileGotoDefinition, SourceRequest(textToFind, offset))
            .Locations.Single();

        var response = Send(Endpoints.DecompileFindUsages, new DecompiledLocationRequest
        {
            Type = LocationType.Decompiled,
            Line = definition.Line,
            Column = definition.Column,
            AssemblyFilePath = definition.AssemblyFilePath,
            ParentAssemblyFilePath = definition.ParentAssemblyFilePath,
            ContainingTypeFullName = definition.ContainingTypeFullName
        });

        var inSource = response.Locations
            .Where(l => l.Type == LocationType.SourceCode)
            .Select(l => l.SourceText.Trim())
            .ToList();
        CollectionAssert.Contains(inSource, expectedLine);
    }

    private static LocationsResponse FindUsages(string textToFind, int offset)
    {
        return Send(Endpoints.DecompileFindUsages, SourceRequest(textToFind, offset));
    }

    private static DecompiledLocationRequest SourceRequest(string textToFind, int offset)
    {
        var lines = File.ReadAllLines(FilePath);
        var lineIndex = Array.FindIndex(lines, l => l.Contains(textToFind));
        Assert.GreaterOrEqual(lineIndex, 0, $"'{textToFind}' not found in {FilePath}");

        return new DecompiledLocationRequest
        {
            FileName = FilePath,
            Type = LocationType.SourceCode,
            Line = lineIndex + 1,
            Column = lines[lineIndex].IndexOf(textToFind, StringComparison.Ordinal) + offset
        };
    }

    private static LocationsResponse Send(string command, DecompiledLocationRequest arguments)
    {
        var response = TestHarness.DotNetCoreIoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(
                new CommandPacket<DecompiledLocationRequest> { Command = command, Arguments = arguments });

        Assert.True(response.Success, response.Message);
        return response.Body;
    }
}
