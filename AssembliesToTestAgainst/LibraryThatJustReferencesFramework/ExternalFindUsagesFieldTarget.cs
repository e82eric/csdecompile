namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesFieldTarget
    {
        private string _field;

        public void Run()
        {
            _field = "0";
            var a = _field;
        }
    }
}