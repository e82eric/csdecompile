using LibraryThatJustReferencesFramework;

public class ExternalFindUsagesMethodOfInterfaceUser2
{
    private readonly ExternalFindUsagesMethodOfInterfaceInterface _externalFindUsagesMethodOfInterfaceInterface2;

    public ExternalFindUsagesMethodOfInterfaceUser2(
        ExternalFindUsagesMethodOfInterfaceInterface externalFindUsagesMethodOfInterfaceInterface2)
    {
        _externalFindUsagesMethodOfInterfaceInterface2 = externalFindUsagesMethodOfInterfaceInterface2;
    }

    public void Run()
    {
        _externalFindUsagesMethodOfInterfaceInterface2.Run();
    }
}