using System.IO;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGetSymbolInfoGenericMethodUser
    {
        public void Run()
        {
            new ExternalGetSymbolInfoGenericMethod().Run(new MemoryStream());
            new ExternalGetSymbolInfoGenericMethod().Run(new MemoryStream(), new MemoryStream());
            new ExternalGetSymbolInfoGenericMethod().Run(new MemoryStream(), "test");
        }
    }
}