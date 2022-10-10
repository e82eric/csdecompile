namespace LibraryThatJustReferencesFramework
{
    public class ExternalSourceGetSymbolInfoUser
    {
        public void Run()
        {
            ExternalSourceGetSymbolInfoTarget obj = new ExternalSourceGetSymbolInfoTarget();
            obj.ExternalRun();
            obj.ExternalBasicProperty = string.Empty;
        }
    }
}