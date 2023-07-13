namespace LibraryThatReferencesLibrary
{
    public class InSourceFindUsagesMethodWithGenericOutParametersTarget<T1, T2>
    {
        public bool TryRun(T1 t1, out T2 t2)
        {
            t2 = default;
            return true;
        }
    }
}