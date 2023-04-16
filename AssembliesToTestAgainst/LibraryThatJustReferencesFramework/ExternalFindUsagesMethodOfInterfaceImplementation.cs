namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesMethodOfInterfaceImplementation : ExternalFindUsagesMethodOfInterfaceInterface
    {
        void ExternalFindUsagesMethodOfInterfaceInterface.Run()
        {
        }

        public void Run2(string p1)
        {
        }

        public void Run2(bool p1)
        {
        }

        public void Run2<T>()
        {
        }

        public void Run2<T>(string p1)
        {
        }

        public void Run2<T>(bool p1)
        {
        }

        public void Run2()
        {
        }
    }
}