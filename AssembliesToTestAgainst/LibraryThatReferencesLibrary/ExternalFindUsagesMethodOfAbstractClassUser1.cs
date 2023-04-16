using LibraryThatJustReferencesFramework;

public class ExternalFindUsagesMethodOfAbstractClassUser1
{
    private readonly ExternalFindUsagesMethodOfAbstractClass _abstractClass;

    public ExternalFindUsagesMethodOfAbstractClassUser1(
        ExternalFindUsagesMethodOfAbstractClass abstractClass)
    {
        _abstractClass = abstractClass;
    }

    public void Run()
    {
        _abstractClass.Run();
    }
}