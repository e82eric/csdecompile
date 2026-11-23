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

    private static LocationsResponse FindUsages(string textToFind, int offset)
    {
        var lines = File.ReadAllLines(FilePath);
        var lineIndex = Array.FindIndex(lines, l => l.Contains(textToFind));
        Assert.GreaterOrEqual(lineIndex, 0, $"'{textToFind}' not found in {FilePath}");

        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileFindUsages,
            Arguments = new DecompiledLocationRequest
            {
                FileName = FilePath,
                Type = LocationType.SourceCode,
                Line = lineIndex + 1,
                Column = lines[lineIndex].IndexOf(textToFind, StringComparison.Ordinal) + offset
            }
        };

        var response = TestHarness.DotNetCoreIoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);

        Assert.True(response.Success, response.Message);
        return response.Body;
    }
}
