using System.Collections.Generic;

namespace CsDecompileLib.DebugClientAssemblySource;

static class UniqTaskStackExtensions
{
    public static void AddIfUniq(this IList<UniqTaskStackItem> uniqTaskStacks, IReadOnlyList<UniqTaskStackFrame> stack, ulong taskAddress)
    {
        var foundMatch = false;
        foreach (var uniqTaskStack in uniqTaskStacks)
        {
            if (stack.Count == uniqTaskStack.Frames.Count)
            {
                var allFramesMatch = true;
                for (var i = 0; i < uniqTaskStack.Frames.Count; i++)
                {
                    if (stack[i].MethodTable != uniqTaskStack.Frames[i].MethodTable)
                    {
                        allFramesMatch = false;
                        break;
                    }
                }

                if (allFramesMatch)
                {
                    uniqTaskStack.Tasks.Add(taskAddress);
                    foundMatch = true;
                    break;
                }
            }
        }

        if (!foundMatch)
        {
            uniqTaskStacks.Add(new UniqTaskStackItem(stack, taskAddress));
        }
    }
}