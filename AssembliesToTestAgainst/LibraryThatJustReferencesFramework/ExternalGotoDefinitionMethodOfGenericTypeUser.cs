namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionMethodOfGenericTypeUser
    {
        public void Run()
        {
            var obj = new ExternalGotoDefinitionMethodOfGenericTypeTarget<string>();
            obj.ExternalBasicMethod(string.Empty);
        }
    }
}