using System;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionOfLambdaParameterTarget
    {
        private void Target()
        {
            Run((i) =>
            {
                var b = i + 1;
                return true;
            });
        }
        public void Run(Func<int, bool> runFunc)
        {
        }
    }
}