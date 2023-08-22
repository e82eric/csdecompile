using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesFrameworkInterfaceTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesStringCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertNumberOfImplementations(
            filePath: FilePath,
            "private string _field;",
            "(?<token>string)",
            "public sealed class String : IComparable, ICloneable, IConvertible, IEnumerable, IComparable<string>, IEnumerable<char>, IEquatable<string>",
            "(?<token>IComparable),",
            column: 13,
            line: 9,
            29);
    }
}