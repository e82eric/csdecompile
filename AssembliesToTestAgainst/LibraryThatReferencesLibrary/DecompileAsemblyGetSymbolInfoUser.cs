using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class DecompileAsemblyGetSymbolInfoUser
    {
        public void Run()
        {
            ExternalSourceGetSymbolInfoTarget obj = new ExternalSourceGetSymbolInfoTarget();
            obj.ExternalRun();
            obj.ExternalBasicProperty = string.Empty;
        }
    }
}