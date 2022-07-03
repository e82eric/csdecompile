namespace LibraryThatJustReferencesFramework
{
    public class ExternalFindUsagesTypeAsGenericUserBase<T>
    {
        public void BaicMethod<T>()
        {
        }

        public T BasicMethod()
        {
            return default;
        }
    }
}