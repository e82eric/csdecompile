using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindUsagesMethodWithGenericInParameterUser<T, T2>
    {
        private ExternalFindUsagesMethodWithGenericInParameterTarget<T, T2> _t1;
        private ExternalFindUsagesMethodWithGenericInParameterTarget<string, object> _t2;

        public void Run()
        {
            T t1 = default;
            T2 t2 = default;
            _t1.TryRun(t1, in t2);

            var o = new object();
            _t2.TryRun("test", in o);
        }
    }
}