﻿using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesEventTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesEventCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 49,
            line: 10,
            
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "new ExternalFindUsagesEventTarget().ExternalBasicEvent += OnExternalBasicEvent;",
                    "ExternalFindUsagesEventCaller"),
                (ResponseLocationType.Decompiled,
                    "externalFindUsagesEventTarget.ExternalBasicEvent += ObjOnExternalBasicEvent;",
                    "ExternalFindUsagesEventCaller"),
                (ResponseLocationType.Decompiled,
                    "externalFindUsagesEventTarget.ExternalBasicEvent -= ObjOnExternalBasicEvent;",
                    "ExternalFindUsagesEventCaller")
            });
    }
}