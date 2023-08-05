using System;

namespace LibraryThatJustReferencesFramework
{
    public class ExternalGotoDefinitionOfLocalFunctionParameterTarget
    {
        public void Run(Func<int, bool> runFunc)
        {
            addOne(1);

            int addOne(int target)
            {
                var result = target + 1;
                return result;
            }
        }
    }
}