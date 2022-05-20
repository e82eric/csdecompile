using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class TopClass1
    {
        public Class1 ClassRef { get; set; }

        public TopClass1()
        {
            ClassRef = new Class1();
        }
    }

    public class TopClass2 : Class1
    {
    }
}