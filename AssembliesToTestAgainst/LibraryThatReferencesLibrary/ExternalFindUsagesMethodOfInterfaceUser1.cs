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
    public void Run2()
    {
        _externalFindUsagesMethodOfInterfaceInterface1.Run2();
    }
    public void Run3()
    {
        _externalFindUsagesMethodOfInterfaceInterface1.Run2("Test");
    }
    public void Run6()
    {
        _externalFindUsagesMethodOfInterfaceInterface1.Run2<string>("Test");
    }
    public void Run7()
    {
        _externalFindUsagesMethodOfInterfaceInterface1.Run2<string>(true);
    }
    public void Run8()
    {
        _externalFindUsagesMethodOfInterfaceInterface1.Run2<string>();
    }
    public void Run9()
    {
        _externalFindUsagesMethodOfInterfaceInterface1.Run2<bool>();
    }
}