namespace LibraryThatReferencesLibrary
{
    public class InSourceFindImplementationsMethodWithGenericOutParametersImplementationWithTypes :
        InSourceFindImplementationsMethodWithGenericOutParametersTarget<string, object>
    {
        public bool TryRun(string t1, out object t2)
        {
            throw new System.NotImplementedException();
        }
    }
}