namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionVariableTarget
    {
        public void Run()
        {
            var targetVar = 0;
            var b = targetVar + 1;
        }
    }
}