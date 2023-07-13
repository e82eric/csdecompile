namespace LibraryThatReferencesLibrary
{
    public class InSourceFindImplementationsMethodWithGenericOutParametersImplementationWithGenerics<T1, T2> :
        InSourceFindImplementationsMethodWithGenericOutParametersTarget<T1, T2>
    {
        public bool TryRun(T1 t1, out T2 t2)
        {
            throw new System.NotImplementedException();
        }
    }
}