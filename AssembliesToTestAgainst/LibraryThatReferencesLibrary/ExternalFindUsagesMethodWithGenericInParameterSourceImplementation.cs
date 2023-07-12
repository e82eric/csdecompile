using System;
using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindUsagesMethodWithGenericInParameterSourceImplementation<T, T2> :
        ExternalFindUsagesMethodWithGenericInParameterTarget<T, T2>
    {
        public bool TryRun(T val, in T2 result)
        {
            throw new NotImplementedException();
        }
    }
}