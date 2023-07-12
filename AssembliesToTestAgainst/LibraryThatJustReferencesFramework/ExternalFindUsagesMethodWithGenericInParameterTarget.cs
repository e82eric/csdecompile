namespace LibraryThatJustReferencesFramework
{
    public interface ExternalFindUsagesMethodWithGenericInParameterTarget<in T, T2>
    {
        bool TryRun(T val, in T2 result);
    }
}