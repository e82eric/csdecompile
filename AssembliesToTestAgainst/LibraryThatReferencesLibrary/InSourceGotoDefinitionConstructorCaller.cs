using System;

namespace LibraryThatReferencesLibrary
{
    public class InSourceGotoDefinitionConstructorCaller
    {
        public void Run()
        {
            new InSourceGotoDefinitionConstructorTarget();
            new InSourceGotoDefinitionConstructorTarget(String.Empty);
            new InSourceGotoDefinitionConstructorTarget(String.Empty, String.Empty);
        }
    }
}