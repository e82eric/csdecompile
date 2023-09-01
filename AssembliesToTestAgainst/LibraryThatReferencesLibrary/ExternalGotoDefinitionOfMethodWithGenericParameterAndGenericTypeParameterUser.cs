using LibraryThatJustReferencesFramework;

namespace LibraryThatReferencesLibrary
{
    public class ExternalGotoDefinitionOfMethodWithGenericParameterAndGenericTypeParameterUser
    {
        public void Run()
        {
            var a = new ExternalGotoDefinitionOfMethodWithGenericParameterAndGenericTypeParameterTarget<string>();
            a.ExternalBasicMethod<int>("Test", 1);
            a.ExternalBasicMethod("Test", 1);
        }
    }
}