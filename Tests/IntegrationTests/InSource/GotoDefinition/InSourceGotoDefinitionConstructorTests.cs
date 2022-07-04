﻿using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionConstructorTests : InSourceBase
{
    private static string FilePath
        = TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionConstructorCaller.cs");
    
    [Test]
    public void NoParameters()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:17,
            line:9,
            expected:"public InSourceGotoDefinitionConstructorTarget()");
    }
    
    [Test]
    public void OneParameter()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:17,
            line:10,
            expected:"public InSourceGotoDefinitionConstructorTarget(string param1)");
    }
    
    [Test]
    public void TwoParameters()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:17,
            line:11,
            expected:"public InSourceGotoDefinitionConstructorTarget(string param1, string param2)");
    }
}