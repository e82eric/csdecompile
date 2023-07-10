namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionMethodWithOutParameterTarget
    {
        public bool TryRun(string val, out string result)
        {
            result = null;
            return true;
        }
    }
}