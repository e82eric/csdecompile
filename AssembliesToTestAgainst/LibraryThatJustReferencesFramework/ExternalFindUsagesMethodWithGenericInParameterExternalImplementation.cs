namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesMethodWithGenericInParameterExternalImplementation<T, T2> :
        ExternalFindUsagesMethodWithGenericInParameterTarget<T, T2>
    {
        public bool TryRun(T val, in T2 result)
        {
            throw new System.NotImplementedException();
        }
    }
}