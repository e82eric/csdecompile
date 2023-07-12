namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindImplementationsMethodWithGenericInParameterExternalImplementation<T, T2> :
        ExternalFindImplementationsMethodWithGenericInParameterTarget<T, T2>
    {
        public bool TryRun(T val, in T2 result)
        {
            throw new System.NotImplementedException();
        }
    }
}