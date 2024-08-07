using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.Util;
using Microsoft.Diagnostics.Runtime;

namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqCallStackProvider
{
    private readonly DataTargetProvider _dataTargetProvider;

    public UniqCallStackProvider(DataTargetProvider dataTargetProvider)
    {
        _dataTargetProvider = dataTargetProvider;
    }
    
    public IReadOnlyList<UniqCallStackItem> Get()
    {
        List<(List<int> metadataTokens, List<ClrThread> threads)> uniqueStacks = new();

        var runtime = _dataTargetProvider.Get().ClrVersions.Single().CreateRuntime();
        foreach (ClrThread thread in runtime.Threads)
        {
            if (!thread.IsAlive)
                continue;

            ClrException? currException = thread.CurrentException;
            if (currException is ClrException ex)
                Console.WriteLine("Exception: {0:X} ({1}), HRESULT={2:X}", ex.Address, ex.Type.Name, ex.HResult);

            var metadataTokens = new List<int>();
            foreach (ClrStackFrame frame in thread.EnumerateStackTrace())
            {
                if (frame.Method != null)
                {
                    metadataTokens.Add(frame.Method.MetadataToken);
                }
            }

            bool found = false;
            foreach (var uniqueStack in uniqueStacks)
            {
                if (metadataTokens.Count == uniqueStack.metadataTokens.Count)
                {
                    var allTokensMatch = true;
                    for (var i = 0; i < metadataTokens.Count; i++)
                    {
                        if (metadataTokens[i] != uniqueStack.metadataTokens[i])
                        {
                            allTokensMatch = false;
                            break;
                        }
                    }

                    if (allTokensMatch)
                    {
                        uniqueStack.threads.Add(thread);
                        found = true;
                    }
                }
            }

            if (found == false)
            {
                uniqueStacks.Add((metadataTokens, new List<ClrThread> {thread}));
            }
        }
            
        uniqueStacks.SortBy(u => u.threads.Count);

        var result = new List<UniqCallStackItem>();
        foreach (var uniqueStack in uniqueStacks)
        {
            var firstThread = uniqueStack.threads.FirstOrDefault();
            if (firstThread != null)
            {
                var frames = new List<UniqCallStackFrame>();
                uint ordinal = 0;
                foreach (var frame in firstThread.EnumerateStackTrace())
                {
                    if (frame.Kind == ClrStackFrameKind.ManagedMethod)
                    {
                        var modelFrame = new UniqCallStackFrame()
                        {
                            InstructionPointer = frame.InstructionPointer,
                            StackPointer = frame.StackPointer,
                            MethodName = frame.Method.Name,
                            TypeName = frame.Method.Type.Name,
                            MetadataToken = frame.Method.MetadataToken,
                            Ordinal = ordinal
                        };
                        ordinal++;
                        frames.Add(modelFrame);
                    }
                }

                var threads = uniqueStack.threads.Select(
                    t => new UniqCallStackThread{ OSId = t.OSThreadId, ManagedId = t.ManagedThreadId}).ToList();
                var item = new UniqCallStackItem { Frames = frames, Threads = threads };
                result.Add(item);
            }
        }

        return result;
    }
}