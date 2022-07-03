namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionFieldTarget
    {
        private string _basicField;

        public void Run()
        {
            _basicField = "0";
            var a = _basicField;
        }
    }
}