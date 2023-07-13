namespace LibraryThatReferencesLibrary
{
    public class InSourceGotoDefinitionMethodWithGenericOutParametersUser
    {
        public void Run<T1, T2>()
        {
            InSourceGotoDefinitionMethodWithGenericOutParametersTarget<T1, T2> a = null;
            a.TryRun(default, out _);
            new InSourceGotoDefinitionMethodWithGenericOutParametersTarget<string, object>().TryRun("test", out _);
        }
    }
}