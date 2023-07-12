namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesMethodWithGenericInParameterExternalUser
    {
        public void Run<T1, T2>()
        {
            ExternalFindUsagesMethodWithGenericInParameterTarget<T1, T2> a = null;
            ExternalFindUsagesMethodWithGenericInParameterTarget<string, object> b = null;
            var r1 = default(T2);
            a.TryRun(default, in r1);
            var r2 = new object();
            b.TryRun("test", in r2);
        }
    }
}