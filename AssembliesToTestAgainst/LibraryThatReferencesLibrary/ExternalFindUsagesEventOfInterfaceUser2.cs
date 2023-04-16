using System;
using LibraryThatJustReferencesFramework;

public class ExternalFindUsagesEventOfInterfaceUser2
{
    private readonly ExternalFindUsagesEventOfInterfaceInterface _externalFindUsagesEventOfInterfaceInterface2;

    public ExternalFindUsagesEventOfInterfaceUser2(
        ExternalFindUsagesEventOfInterfaceInterface externalFindUsagesEventOfInterfaceInterface2)
    {
        _externalFindUsagesEventOfInterfaceInterface2 = externalFindUsagesEventOfInterfaceInterface2;
    }

    public void Run()
    {
        _externalFindUsagesEventOfInterfaceInterface2.Event1 += ExternalFindUsagesEventOfInterfaceInterface2OnEvent2;
    }

    private void ExternalFindUsagesEventOfInterfaceInterface2OnEvent2(object sender, EventArgs e)
    {
    }
}