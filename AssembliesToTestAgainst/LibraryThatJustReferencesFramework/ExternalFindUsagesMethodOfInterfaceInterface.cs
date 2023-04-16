namespace LibraryThatJustReferencesFramework
{
    public interface ExternalFindUsagesMethodOfInterfaceInterface
    {
        void Run();
        void Run2(string p1);
        void Run2(bool p1);
        void Run2<T>();
        void Run2<T>(string p1);
        void Run2<T>(bool p1);
        void Run2();
    }
}