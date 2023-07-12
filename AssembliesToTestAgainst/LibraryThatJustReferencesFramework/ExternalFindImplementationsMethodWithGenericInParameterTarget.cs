namespace LibraryThatJustReferencesFramework
{
    public interface ExternalFindImplementationsMethodWithGenericInParameterTarget<in T, T2>
    {
        bool TryRun(T val, in T2 result);
    }
}