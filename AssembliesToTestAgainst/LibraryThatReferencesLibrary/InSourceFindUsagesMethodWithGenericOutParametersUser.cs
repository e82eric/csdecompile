namespace LibraryThatReferencesLibrary
{
    public class InSourceFindUsagesMethodWithGenericOutParametersUser
    {
        public void Run<T1, T2>()
        {
            InSourceFindUsagesMethodWithGenericOutParametersTarget<T1, T2> a = null;
            a.TryRun(default, out _);
            var b = new InSourceFindUsagesMethodWithGenericOutParametersTarget<string, object>();
            b.TryRun(default, out _);
        }
    }
}