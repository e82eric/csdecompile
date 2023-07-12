using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalFindImplementationsMethodWithGenericInParameterSourceUser<T, T2>
    {
        private ExternalFindImplementationsMethodWithGenericInParameterTarget<T, T2> _t1;
        private ExternalFindImplementationsMethodWithGenericInParameterTarget<string, object> _t2;

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