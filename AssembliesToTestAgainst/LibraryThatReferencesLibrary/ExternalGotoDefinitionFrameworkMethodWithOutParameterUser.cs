using System.Collections.Generic;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionFrameworkMethodWithOutParameterUser
    {
        public void Run()
        {
            new Dictionary<string, string>().TryGetValue("test", out _);
        }
    }
}