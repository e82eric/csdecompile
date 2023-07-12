using System;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionMethodWithGenericInParameterTarget<T, T2>
    {
        public bool TryRun(T val, in T2 result)
        {
            return true;
        }
    }
}