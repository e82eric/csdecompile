namespace LibraryThatJustReferencesFramework
{
    public class Class1
    {
       public string EricString { get; set; }
       public int EricInt { get; set; }
    }

    public class Class2
    {
        public Class1 Execute()
        {
            return new Class1()
            {
                EricString = "Test",
                EricInt = 1
            };
        }
    }

    internal class Class3
    {
        public Class2 Execute()
        {
            return new Class2();
        }
    }
}