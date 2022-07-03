namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesTypeAsFieldUser
    {
        private ExternalFindUsagesTypeAsFieldTarget _field;

        public void Run()
        {
            _field = new ExternalFindUsagesTypeAsFieldTarget();
            ExternalFindUsagesTypeAsFieldTarget a = _field;
        }
    }
}