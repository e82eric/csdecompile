namespace LibraryThatReferencesLibrary
{
    public class InSourceGotoDefinitionMethodWithGenericParametersUser
    {
        public void Run()
        {
            new InSourceGotoDefinitionMethodWithGenericParametersTarget<string, object>().TryRun("Test", new object());
        }
    }
}