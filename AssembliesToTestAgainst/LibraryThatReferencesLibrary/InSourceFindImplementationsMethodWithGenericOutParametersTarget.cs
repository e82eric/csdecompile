namespace LibraryThatReferencesLibrary
{
    public interface InSourceFindImplementationsMethodWithGenericOutParametersTarget<T1, T2>
    {
        bool TryRun(T1 t1, out T2 t2);
    }
}