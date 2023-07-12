using System;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindImplementationsMethodWithGenericInParameterSourceImplementation<T, T2> :
        ExternalFindImplementationsMethodWithGenericInParameterTarget<T, T2>
    {
        public bool TryRun(T val, in T2 result)
        {
            throw new NotImplementedException();
        }
    }
}