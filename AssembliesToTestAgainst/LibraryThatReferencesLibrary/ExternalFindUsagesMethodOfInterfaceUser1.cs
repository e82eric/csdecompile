using LibraryThatJustReferencesFramework;

public class ExternalFindUsagesMethodOfInterfaceUser1
{
    private readonly ExternalFindUsagesMethodOfInterfaceInterface _externalFindUsagesMethodOfInterfaceInterface1;

    public ExternalFindUsagesMethodOfInterfaceUser1(
        ExternalFindUsagesMethodOfInterfaceInterface externalFindUsagesMethodOfInterfaceInterface1)
    {
        _externalFindUsagesMethodOfInterfaceInterface1 = externalFindUsagesMethodOfInterfaceInterface1;
    }

    public void Run()
    {
        _externalFindUsagesMethodOfInterfaceInterface1.Run();
    }
}