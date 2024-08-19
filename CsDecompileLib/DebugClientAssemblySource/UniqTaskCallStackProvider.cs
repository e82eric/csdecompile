using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.Util;
using Microsoft.Diagnostics.Runtime;

namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqTaskCallStackProvider
{
    private readonly DataTargetProvider _dataTargetProvider;
    private readonly NetCoreDumpAsyncCommand _netCoreHandler;

    public UniqTaskCallStackProvider(DataTargetProvider dataTargetProvider)
    {
        _dataTargetProvider = dataTargetProvider;
        _netCoreHandler = new NetCoreDumpAsyncCommand();
    }
    
    private static bool IsNetCore(ClrRuntime runtime)
    {
        foreach (ClrModule module in runtime.EnumerateModules())
        {
            if (string.IsNullOrEmpty(module.AssemblyName))
                continue;

            var name = module.AssemblyName.ToLower();
            if (name.Contains("corelib"))
            {
                return true;
            }
        }

        return false;
    }
    
    public IReadOnlyList<UniqTaskStackItem> Get()
    {
        var runtime = _dataTargetProvider.Get().ClrVersions.Single().CreateRuntime();
        if (IsNetCore(runtime))
        {
            return _netCoreHandler.Invoke(runtime);
        }
        ClrHeap heap = runtime.Heap;

        var allStateMachines = new List<AsyncStateMachine>();
        var knownStateMachines = new Dictionary<ulong, AsyncStateMachine>();

        GetAllStateMachines(heap, allStateMachines, knownStateMachines);

        ChainStateMachinesBasedOnTaskContinuations(knownStateMachines);
        ChainStateMachinesBasedOnJointableTasks(allStateMachines);
        MarkThreadingBlockTasks(allStateMachines, new []{runtime});
        MarkUIThreadDependingTasks(allStateMachines);
        FixBrokenDependencies(allStateMachines);
        PrintOutStateMachines(allStateMachines);
            
        IOrderedEnumerable<AsyncStateMachine> rootStateMachines = allStateMachines
            .Where(m => m.Depth > 0)
            .OrderByDescending(m => m.Depth)
            .ThenByDescending(m => m.SwitchToMainThreadTask.Address);

        var result = GetUniqCallStacks(rootStateMachines);
        return result;
    }

    private static void GetAllStateMachines(ClrHeap heap, List<AsyncStateMachine> allStateMachines,
        Dictionary<ulong, AsyncStateMachine> knownStateMachines)
    {
        foreach (ClrObject obj in Utilities.GetObjectsOfType(heap,
                     "System.Runtime.CompilerServices.AsyncMethodBuilderCore+MoveNextRunner"))
        {
            try
            {
                ClrObject stateMachine = obj.ReadObjectField("m_stateMachine");
                if (!knownStateMachines.ContainsKey(stateMachine.Address))
                {
                    try
                    {
                        var state = stateMachine.ReadField<int>("<>1__state");
                        if (state >= -1)
                        {
                            ClrObject taskField = default(ClrObject);
                            ClrValueType? asyncBuilder = stateMachine.TryGetValueClassField("<>t__builder");
                            if (asyncBuilder.HasValue)
                            {
                                while (asyncBuilder.HasValue)
                                {
                                    taskField = asyncBuilder.TryGetObjectField("m_task");
                                    if (!taskField.IsNull)
                                    {
                                        break;
                                    }

                                    ClrValueType? nextAsyncBuilder = asyncBuilder.TryGetValueClassField("m_builder");
                                    if (nextAsyncBuilder is null)
                                    {
                                        asyncBuilder = asyncBuilder.TryGetValueClassField("_methodBuilder");
                                    }
                                    else
                                    {
                                        asyncBuilder = nextAsyncBuilder;
                                    }
                                }
                            }
                            else
                            {
                                // CLR debugger may not be able to access t__builder, when NGEN assemblies are being used, and the type of the field could be lost.
                                // Our workaround is to pick up the first Task object referenced by the state machine, which seems to be correct.
                                // That function works with the raw data structure (like how GC scans the object, so it doesn't depend on symbols.
                                //
                                // However, one problem of that is we can pick up tasks from other reference fields of the same structure. So, we go through fields which we have symbols
                                // and remember references encounted, and we skip them when we go through GC references.
                                // Note: we can do better by going through other value structures, and extract references from them here, which we can consider when we have a real scenario.
                                var previousReferences = new Dictionary<ulong, int>();
                                if (stateMachine.Type?.GetFieldByName("<>t__builder") is not null)
                                {
                                    foreach (ClrInstanceField field in stateMachine.Type.Fields)
                                    {
                                        if (string.Equals(field.Name, "<>t__builder", StringComparison.Ordinal))
                                        {
                                            break;
                                        }

                                        if (field.IsObjectReference)
                                        {
                                            ClrObject referencedValue =
                                                field.ReadObject(stateMachine.Address, interior: false);
                                            if (!referencedValue.IsNull)
                                            {
                                                if (previousReferences.TryGetValue(referencedValue.Address,
                                                        out int refCount))
                                                {
                                                    previousReferences[referencedValue.Address] = refCount + 1;
                                                }
                                                else
                                                {
                                                    previousReferences[referencedValue.Address] = 1;
                                                }
                                            }
                                        }
                                    }
                                }

                                foreach (ClrObject referencedObject in stateMachine.EnumerateReferences(true))
                                {
                                    if (!referencedObject.IsNull)
                                    {
                                        if (previousReferences.TryGetValue(referencedObject.Address,
                                                out int refCount) && refCount > 0)
                                        {
                                            if (refCount == 1)
                                            {
                                                previousReferences.Remove(referencedObject.Address);
                                            }
                                            else
                                            {
                                                previousReferences[referencedObject.Address] = refCount - 1;
                                            }

                                            continue;
                                        }
                                        else if (previousReferences.Count > 0)
                                        {
                                            continue;
                                        }

                                        if (referencedObject.Type is object &&
                                            (string.Equals(referencedObject.Type.Name, "System.Threading.Tasks.Task",
                                                 StringComparison.Ordinal) ||
                                             string.Equals(referencedObject.Type.BaseType?.Name,
                                                 "System.Threading.Tasks.Task", StringComparison.Ordinal)))
                                        {
                                            taskField = referencedObject;
                                            break;
                                        }
                                    }
                                }
                            }

                            var asyncState = new AsyncStateMachine(state, stateMachine, taskField);
                            allStateMachines.Add(asyncState);
                            knownStateMachines.Add(stateMachine.Address, asyncState);

                            if (stateMachine.Type is object)
                            {
                                foreach (ClrMethod? method in stateMachine.Type.Methods)
                                {
                                    if (method.Name == "MoveNext" && method.NativeCode != ulong.MaxValue)
                                    {
                                        asyncState.CodeAddress = method.NativeCode;
                                    }
                                }
                            }
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        Console.WriteLine(
                            $"Fail to process state machine {stateMachine.Address:x} Type:'{stateMachine.Type?.Name}' Module:'{stateMachine.Type?.Module?.Name}' Error: {ex.Message}");
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Console.WriteLine($"Fail to process AsyncStateMachine Runner {obj.Address:x} Error: {ex.Message}");
            }
        }
    }

    private static void ChainStateMachinesBasedOnTaskContinuations(
        Dictionary<ulong, AsyncStateMachine> knownStateMachines)
    {
        foreach (AsyncStateMachine? stateMachine in knownStateMachines.Values)
        {
            ClrObject taskObject = stateMachine.Task;
            try
            {
                while (!taskObject.IsNull)
                {
                    // 3 cases in order to get the _target:
                    // 1. m_continuationObject.m_action._target
                    // 2. m_continuationObject._target
                    // 3. m_continuationObject.m_task.m_stateObject._target
                    ClrObject continuationObject = taskObject.TryGetObjectField("m_continuationObject");
                    if (continuationObject.IsNull)
                    {
                        break;
                    }

                    ChainStateMachineBasedOnTaskContinuations(knownStateMachines, stateMachine, continuationObject);

                    taskObject = continuationObject;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Console.WriteLine(
                    $"Fail to fix continuation of state {stateMachine.StateMachine.Address:x} Error: {ex.Message}");
            }
        }
    }

    private static void ChainStateMachineBasedOnTaskContinuations(
        Dictionary<ulong, AsyncStateMachine> knownStateMachines, AsyncStateMachine stateMachine,
        ClrObject continuationObject)
    {
        ClrObject continuationAction = continuationObject.TryGetObjectField("m_action");

        // case 1
        ClrObject continuationTarget = continuationAction.TryGetObjectField("_target");
        if (continuationTarget.IsNull)
        {
            // case 2
            continuationTarget = continuationObject.TryGetObjectField("_target");
            if (continuationTarget.IsNull)
            {
                // case 3
                continuationTarget = continuationObject.TryGetObjectField("m_task").TryGetObjectField("m_stateObject")
                    .TryGetObjectField("_target");
            }
        }

        while (!continuationTarget.IsNull)
        {
            // now get the continuation from the target
            ClrObject continuationTargetStateMachine = continuationTarget.TryGetObjectField("m_stateMachine");
            if (!continuationTargetStateMachine.IsNull)
            {
                if (knownStateMachines.TryGetValue(continuationTargetStateMachine.Address,
                        out AsyncStateMachine? targetAsyncState) && targetAsyncState != stateMachine)
                {
                    stateMachine.Next = targetAsyncState;
                    stateMachine.DependentCount++;
                    targetAsyncState.Previous = stateMachine;
                }

                break;
            }
            else
            {
                ClrObject nextContinuation = continuationTarget.TryGetObjectField("m_continuation");
                continuationTarget = nextContinuation.TryGetObjectField("_target");
            }
        }

        ClrObject items = continuationObject.TryGetObjectField("_items");
        if (!items.IsNull && items.IsArray && items.ContainsPointers)
        {
            foreach (ClrObject promise in items.EnumerateReferences(true))
            {
                if (!promise.IsNull)
                {
                    ClrObject innerContinuationObject = promise.TryGetObjectField("m_continuationObject");
                    if (!innerContinuationObject.IsNull)
                    {
                        ChainStateMachineBasedOnTaskContinuations(knownStateMachines, stateMachine,
                            innerContinuationObject);
                    }
                    else
                    {
                        ChainStateMachineBasedOnTaskContinuations(knownStateMachines, stateMachine, promise);
                    }
                }
            }
        }
    }

    private static void ChainStateMachinesBasedOnJointableTasks(List<AsyncStateMachine> allStateMachines)
    {
        foreach (AsyncStateMachine? stateMachine in allStateMachines)
        {
            if (stateMachine.Previous is null)
            {
                try
                {
                    ClrObject joinableTask = stateMachine.StateMachine.TryGetObjectField("<>4__this");
                    ClrObject wrappedTask = joinableTask.TryGetObjectField("wrappedTask");
                    if (!wrappedTask.IsNull)
                    {
                        AsyncStateMachine? previousStateMachine = allStateMachines
                            .FirstOrDefault(s => s.Task.Address == wrappedTask.Address);
                        if (previousStateMachine is object && stateMachine != previousStateMachine)
                        {
                            stateMachine.Previous = previousStateMachine;
                            previousStateMachine.Next = stateMachine;
                            previousStateMachine.DependentCount++;
                        }
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    Console.WriteLine(
                        $"Fail to fix continuation of state {stateMachine.StateMachine.Address:x} Error: {ex.Message}");
                }
            }
        }
    }

    private static void MarkUIThreadDependingTasks(List<AsyncStateMachine> allStateMachines)
    {
        foreach (AsyncStateMachine? stateMachine in allStateMachines)
        {
            if (stateMachine.Previous is null && stateMachine.State >= 0)
            {
                try
                {
                    ClrInstanceField? awaitField =
                        stateMachine.StateMachine.Type?.GetFieldByName($"<>u__{stateMachine.State + 1}");
                    if (awaitField is object && awaitField.IsValueType && string.Equals(awaitField.Type?.Name,
                            "Microsoft.VisualStudio.Threading.JoinableTaskFactory+MainThreadAwaiter",
                            StringComparison.Ordinal))
                    {
                        ClrValueType? awaitObject =
                            stateMachine.StateMachine.TryGetValueClassField($"<>u__{stateMachine.State + 1}");
                        if (awaitObject.HasValue)
                        {
                            stateMachine.SwitchToMainThreadTask = awaitObject.TryGetObjectField("job");
                        }
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                }
            }
        }
    }

    private static void FixBrokenDependencies(List<AsyncStateMachine> allStateMachines)
    {
        foreach (AsyncStateMachine? stateMachine in allStateMachines)
        {
            if (stateMachine.Previous is object && stateMachine.Previous.Next != stateMachine)
            {
                // If the previous task actually has two continuations, we end up in a one way dependencies chain, we need fix it in the future.
                stateMachine.AlterPrevious = stateMachine.Previous;
                stateMachine.Previous = null;
            }
        }
    }

    private void MarkThreadingBlockTasks(List<AsyncStateMachine> allStateMachines, IList<ClrRuntime> runtimes)
    {
        foreach (ClrRuntime runtime in runtimes)
        {
            foreach (ClrThread? thread in runtime.Threads)
            {
                ClrStackFrame? stackFrame = thread.EnumerateStackTrace().Take(50).FirstOrDefault(
                    f => f.Method is { } method
                         && string.Equals(f.Method.Name, "CompleteOnCurrentThread", StringComparison.Ordinal)
                         && string.Equals(f.Method.Type?.Name, "Microsoft.VisualStudio.Threading.JoinableTask",
                             StringComparison.Ordinal));

                if (stackFrame is object)
                {
                    var visitedObjects = new HashSet<ulong>();
                    foreach (ClrStackRoot stackRoot in thread.EnumerateStackRoots())
                    {
                        ClrObject stackObject = stackRoot.Object;
                        if (string.Equals(stackObject.Type?.Name, "Microsoft.VisualStudio.Threading.JoinableTask",
                                StringComparison.Ordinal) ||
                            string.Equals(stackObject.Type?.BaseType?.Name,
                                "Microsoft.VisualStudio.Threading.JoinableTask", StringComparison.Ordinal))
                        {
                            if (visitedObjects.Add(stackObject.Address))
                            {
                                ClrObject joinableTaskObject = stackObject.Type is null
                                    ? runtime.Heap.GetObject(stackObject.Address)
                                    : runtime.Heap.GetObject(stackObject.Address, stackObject.Type);
                                int state = joinableTaskObject.ReadField<int>("state");
                                if ((state & 0x10) == 0x10)
                                {
                                    // This flag indicates the JTF is blocking the thread
                                    ClrObject wrappedTask = joinableTaskObject.TryGetObjectField("wrappedTask");
                                    if (!wrappedTask.IsNull)
                                    {
                                        AsyncStateMachine? blockingStateMachine = allStateMachines
                                            .FirstOrDefault(s => s.Task.Address == wrappedTask.Address);
                                        if (blockingStateMachine is object)
                                        {
                                            blockingStateMachine.BlockedThread = thread.OSThreadId;
                                            blockingStateMachine.BlockedJoinableTask = joinableTaskObject;
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    private IReadOnlyList<UniqTaskStackItem> GetUniqCallStacks(IOrderedEnumerable<AsyncStateMachine> rootStateMachines)
    {
        List<UniqTaskStackItem> uniqTaskStacks = new();

        foreach (var rootStateMachine in rootStateMachines)
        {
            IReadOnlyList<UniqTaskStackFrame> stack = rootStateMachine.ToTaskCallStack();

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
                        uniqTaskStack.Tasks.Add(rootStateMachine.Task.Address);
                        foundMatch = true;
                        break;
                    }
                }
            }

            if (!foundMatch)
            {
                uniqTaskStacks.Add(new UniqTaskStackItem(stack, rootStateMachine.Task.Address));
            }
        }

        uniqTaskStacks.SortBy(u => u.Tasks.Count);
        return uniqTaskStacks;
    }

    private void PrintOutStateMachines(List<AsyncStateMachine> allStateMachines)
    {
        int loopMark = -1;
        foreach (AsyncStateMachine? stateMachine in allStateMachines)
        {
            int depth = 0;
            if (stateMachine.Previous is null)
            {
                AsyncStateMachine? p = stateMachine;
                while (p is object)
                {
                    depth++;
                    if (p.Depth == loopMark)
                    {
                        break;
                    }

                    p.Depth = loopMark;
                    p = p.Next;
                }
            }

            if (stateMachine.AlterPrevious is object)
            {
                depth++;
            }

            stateMachine.Depth = depth;
            loopMark--;
        }
    }

    protected void WriteString(string message)
    {
        Console.WriteLine(message);
    }

    protected void WriteLine(string message)
    {
        this.WriteString(message + "\n");
    }

    protected void WriteObjectAddress(ulong address)
    {
        Console.WriteLine(address.ToString("x"));
    }

    protected void WriteThreadLink(uint threadId)
    {
        Console.WriteLine($"Thread TID:[{threadId:x}]");
    }

    protected void WriteMethodInfo(string name, ulong address)
    {
        Console.WriteLine(name);
    }

    protected void WriteStringWithLink(string message, string linkCommand)
    {
        Console.WriteLine(message);
    }

    private class AsyncStateMachine
    {
        public AsyncStateMachine(int state, ClrObject stateMachine, ClrObject task)
        {
            this.State = state;
            this.StateMachine = stateMachine;
            this.Task = task;
        }

        public int State { get; } // -1 == currently running, 0 = still waiting on first await, 2= before the 3rd await

        public ClrObject StateMachine { get; }

        public ClrObject Task { get; }

        public AsyncStateMachine? Previous { get; set; }

        public AsyncStateMachine? Next { get; set; }

        public int DependentCount { get; set; }

        public int Depth { get; set; }

        public uint? BlockedThread { get; set; }

        public ClrObject BlockedJoinableTask { get; set; }

        public ClrObject SwitchToMainThreadTask { get; set; }

        public AsyncStateMachine? AlterPrevious { get; set; }

        public ulong CodeAddress { get; set; }

        public override string ToString()
        {
            return $"state = {this.State} Depth {this.Depth} StateMachine = {this.StateMachine} Task = {this.Task}";
        }

        public string PrintCallStack()
        {
            var toConcat = new List<string>();
            toConcat.Add($"{"MethodTable", -12} {"InsPtr", -12} State Name");
            var current = this;
            while (current != null)
            {
                if (current.StateMachine.Type != null)
                {
                    var state = current.StateMachine.ReadField<Int32>("<>1__state");

                    toConcat.Add(
                        current.StateMachine.Type.MethodTable.ToString("x8") +
                        " " + current.CodeAddress.ToString("x8") +
                        " " + $"{state, 5}" +
                        " " + current.StateMachine.Type.Name);
                }

                current = current.Next;
            } 
            
            var result = String.Join("\n", toConcat);
            return result;
        }

        
        public IReadOnlyList<UniqTaskStackFrame> ToTaskCallStack()
        {
            var result = new List<UniqTaskStackFrame>();
            var current = this;
            uint ordinal = 0;
            while (current != null)
            {
                if (current.StateMachine.Type != null)
                {
                    var state = current.StateMachine.ReadField<Int32>("<>1__state");
                    var frame = new UniqTaskStackFrame
                    {
                        Ordinal = ordinal,
                        InstructionPointer = current.CodeAddress,
                        MethodTable = current.StateMachine.Type.MethodTable,
                        StateMachineTypeName = current.StateMachine.Type?.Name,
                        State = state
                    };
                    result.Add(frame);
                    ordinal++;
                }

                current = current.Next;
            } 
            
            return result;
        }
    }
}