using LibraryThatJustReferencesFramework;

public class ExternalFindUsagesMethodOfAbstractClassUser2
{
    private readonly ExternalFindUsagesMethodOfAbstractClass _abstractClass;

    public ExternalFindUsagesMethodOfAbstractClassUser2(
        ExternalFindUsagesMethodOfAbstractClass abstractClass)
    {
        _abstractClass = abstractClass;
    }

    public void Run()
    {
        _abstractClass.Run();
    }
}