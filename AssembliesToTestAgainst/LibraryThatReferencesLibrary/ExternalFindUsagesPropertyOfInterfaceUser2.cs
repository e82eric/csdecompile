using LibraryThatJustReferencesFramework;

public class ExternalFindUsagesPropertyOfInterfaceUser2
{
    private readonly ExternalFindUsagesPropertyOfInterfaceInterface _externalFindUsagesPropertyOfInterfaceInterface2;

    public ExternalFindUsagesPropertyOfInterfaceUser2(
        ExternalFindUsagesPropertyOfInterfaceInterface externalFindUsagesPropertyOfInterfaceInterface2)
    {
        _externalFindUsagesPropertyOfInterfaceInterface2 = externalFindUsagesPropertyOfInterfaceInterface2;
    }

    public void Run()
    {
        var t1 = _externalFindUsagesPropertyOfInterfaceInterface2.Prop1;
    }
}