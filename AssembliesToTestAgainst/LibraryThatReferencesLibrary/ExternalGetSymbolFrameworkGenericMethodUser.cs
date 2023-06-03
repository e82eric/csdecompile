using System.IO;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGetSymbolFrameworkGenericMethodUser
    {
        public void Run()
        {
            new ExternalGetSymbolFrameworkGenericMethodTarget().Items.Add(new MemoryStream());
        }
    }
}