﻿using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

public class EventUsedByMetadataScanner : MemberUsedByMetadataScanner
{
    public EventUsedByMetadataScanner(AnalyzerScope analyzerScope):base(analyzerScope)
    {
    }

    protected override bool IsSameMember(IMember analyzedMethod, IMember m)
    {
        var isSameMember = base.IsSameMember(analyzedMethod, m);
        if (isSameMember)
        {
            return true;
        }
        
        if (analyzedMethod is IEvent @event)
        {
            if (@event.CanAdd)
            {
                if (@event.AddAccessor != null)
                {
                    var isSameAdder = m.AreSameUsingToken(@event.AddAccessor);

                    if (isSameAdder)
                    {
                        return true;
                    }
                }
            }

            if (@event.CanRemove)
            {
                var isSameRemover = m.AreSameUsingToken(@event.RemoveAccessor);

                if (isSameRemover)
                {
                    return true;
                }
            }

            if (@event.CanInvoke)
            {
                var isSameInvoker = m.AreSameUsingToken(@event.InvokeAccessor);

                if (isSameInvoker)
                {
                    return true;
                }
            }
        }

        return false;
    }
}