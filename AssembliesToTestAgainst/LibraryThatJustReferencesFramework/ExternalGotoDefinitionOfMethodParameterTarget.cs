namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionOfMethodParameterTarget
    {
        public void Run(int param1, int param2)
        {
            var a = param1 + 1;
            var b = param2 + 2;
        }
    }
}