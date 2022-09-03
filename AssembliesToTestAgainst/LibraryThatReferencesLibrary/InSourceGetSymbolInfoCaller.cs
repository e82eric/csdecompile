namespace LibraryThatReferencesLibrary
{
    public class InSourceGetSymbolInfoCaller
    {
        public void Run()
        {
            InSourceGetSymbolInfoTarget obj = new InSourceGetSymbolInfoTarget();
            obj.Run();
            obj.BasicProperty = string.Empty;
        }
    }
}