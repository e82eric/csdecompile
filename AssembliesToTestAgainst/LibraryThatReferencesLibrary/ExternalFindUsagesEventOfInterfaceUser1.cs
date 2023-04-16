using System;
using LibraryThatJustReferencesFramework;

public class ExternalFindUsagesEventOfInterfaceUser1
{
    private readonly ExternalFindUsagesEventOfInterfaceInterface _externalFindUsagesEventOfInterfaceInterface1;

    public ExternalFindUsagesEventOfInterfaceUser1(
        ExternalFindUsagesEventOfInterfaceInterface externalFindUsagesEventOfInterfaceInterface1)
    {
        _externalFindUsagesEventOfInterfaceInterface1 = externalFindUsagesEventOfInterfaceInterface1;
    }

    public void Run()
    {
        _externalFindUsagesEventOfInterfaceInterface1.Event1 += ExternalFindUsagesEventOfInterfaceInterface1OnEvent1;
    }

    private void ExternalFindUsagesEventOfInterfaceInterface1OnEvent1(object sender, EventArgs e)
    {
    }
}