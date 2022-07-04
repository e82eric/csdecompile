﻿using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsVariableTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsVariableCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
            expected: new []
            {
                (ResponseLocationType.SourceCode, "ExternalFindUsagesTypeAsVariableTarget a = null;"),
                (ResponseLocationType.Decompiled, "ExternalFindUsagesTypeAsVariableCaller externalFindUsagesTypeAsVariableCaller = null;"),
                (ResponseLocationType.Decompiled, "ExternalFindUsagesTypeAsVariableCaller externalFindUsagesTypeAsVariableCaller2 = null;"),
                (ResponseLocationType.Decompiled, "ExternalFindUsagesTypeAsVariableCaller externalFindUsagesTypeAsVariableCaller3 = null;"),
            });
    }
}