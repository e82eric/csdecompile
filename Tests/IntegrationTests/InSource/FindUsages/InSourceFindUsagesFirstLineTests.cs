using System.Linq;
using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceFindUsagesFirstLineTests
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceFindUsagesFirstLineUser.cs");

    private static LocationsResponse FindUsagesOfTarget()
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileFindUsages,
            Arguments = new DecompiledLocationRequest
            {
                FileName = FilePath,
                Column = 12,
                Type = LocationType.SourceCode,
                Line = 9
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);

        Assert.True(response.Success, response.Message);
        return response.Body;
    }

    [Test]
    public void UsageOnTheFirstLineOfAFileIsReturned()
    {
        var locations = FindUsagesOfTarget().Locations;

        var firstLine = locations.Single(l => l.Line == 1 && ((SourceFileInfo)l).FileName == FilePath);
        StringAssert.StartsWith("using InSourceFindUsagesFirstLineAlias", firstLine.SourceText);
        Assert.AreEqual(3, locations.Count);
    }

    [Test]
    public void ContainingTypeIsTheTypeTheUsageIsIn()
    {
        var locations = FindUsagesOfTarget().Locations;

        var inStruct = locations.Single(l => l.SourceText == "InSourceFindUsagesFirstLineTarget a;");
        Assert.AreEqual("InSourceFindUsagesFirstLineUser", inStruct.ContainingTypeShortName);
        Assert.AreEqual(
            "LibraryThatReferencesLibrary.InSourceFindUsagesFirstLineUser",
            inStruct.ContainingTypeFullName);
    }
}
