using LibraryThatJustReferencesFramework;

public class ExternalFindUsagesPropertyOfInterfaceUser1
{
    private readonly ExternalFindUsagesPropertyOfInterfaceInterface _externalFindUsagesPropertyOfInterfaceInterface1;

    public ExternalFindUsagesPropertyOfInterfaceUser1(
        ExternalFindUsagesPropertyOfInterfaceInterface externalFindUsagesPropertyOfInterfaceInterface1)
    {
        _externalFindUsagesPropertyOfInterfaceInterface1 = externalFindUsagesPropertyOfInterfaceInterface1;
    }

    public void Run()
    {
        var t1 = _externalFindUsagesPropertyOfInterfaceInterface1.Prop1;
    }
}