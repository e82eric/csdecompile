namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionMethodWithGenericOutParameterTarget<T, T2>
    {
        public bool TryRun(T val, out T2 result)
        {
            result = default;
            return true;
        }
    }
}