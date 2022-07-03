namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesVariableTarget
    {
        public void Run()
        {
            var basicVar = 1;
            var b = basicVar + 1;
            var c = basicVar + basicVar;
        }
    }
}