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
    public void Run2()
    {
        _externalFindUsagesMethodOfInterfaceInterface2.Run2();
    }
    public void Run5()
    {
        _externalFindUsagesMethodOfInterfaceInterface2.Run2<string>();
    }
    public void Run8()
    {
        _externalFindUsagesMethodOfInterfaceInterface2.Run2<bool>();
    }
    public void Run6()
    {
        _externalFindUsagesMethodOfInterfaceInterface2.Run2<string>("Test");
    }
    public void Run7()
    {
        _externalFindUsagesMethodOfInterfaceInterface2.Run2<string>(true);
    }
    public void Run3()
    {
        _externalFindUsagesMethodOfInterfaceInterface2.Run2("Test");
    }
}