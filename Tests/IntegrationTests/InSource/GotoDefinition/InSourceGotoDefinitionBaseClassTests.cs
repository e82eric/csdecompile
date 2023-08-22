using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class InSourceGotoDefinitionBaseClassTests : InSourceBase
    {
        private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
            "InSourceGotoDefinitionBaseClassCaller.cs");
        
        [Test]
        public void GotoInSourceClassDefinition()
        {
            RequestAndAssertCorrectLine(
                filePath:FilePath,
                column:58,
                line: 3,
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class InSourceGotoDefinitionBaseClassTarget",
                    null,
                    "LibraryThatReferencesLibrary.InSourceGotoDefinitionBaseClassTarget"));
        }
    }
}