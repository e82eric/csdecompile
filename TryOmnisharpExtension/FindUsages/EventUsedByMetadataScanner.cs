using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class EventUsedByMetadataScanner : MemberUsedByMetadataScanner
{
    [ImportingConstructor]
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
                    var isSameAdder = m.MetadataToken == @event.AddAccessor.MetadataToken
                                       && m.ParentModule.PEFile == @event.ParentModule.PEFile;

                    if (isSameAdder)
                    {
                        return true;
                    }
                }
            }

            if (@event.CanRemove)
            {
                var isSameRemover = m.MetadataToken == @event.RemoveAccessor.MetadataToken
                                   && m.ParentModule.PEFile == @event.ParentModule.PEFile;

                if (isSameRemover)
                {
                    return true;
                }
            }

            if (@event.CanInvoke)
            {
                var isSameInvoker = m.MetadataToken == @event.InvokeAccessor.MetadataToken
                                   && m.ParentModule.PEFile == @event.ParentModule.PEFile;

                if (isSameInvoker)
                {
                    return true;
                }
            }
        }

        return false;
    }
}