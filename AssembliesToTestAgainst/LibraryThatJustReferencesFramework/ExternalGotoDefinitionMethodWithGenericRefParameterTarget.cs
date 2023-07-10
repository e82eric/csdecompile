namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionMethodWithGenericRefParameterTarget<T, T2>
    {
        public bool TryRun(T val, ref T2 result)
        {
            return true;
        }
    }
}