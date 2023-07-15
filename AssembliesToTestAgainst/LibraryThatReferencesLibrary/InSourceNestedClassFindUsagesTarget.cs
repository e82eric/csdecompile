namespace LibraryThatReferencesLibrary
{
    public class InSourceNestedClassFindUsagesTarget
    {
        public class InnerClass
        {
        }

        public void Run()
        {
            InnerClass b;
        }
    }
}
