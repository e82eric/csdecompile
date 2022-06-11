using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class PropertyUsedByMetadataScanner : MemberUsedByMetadataScanner
{
    [ImportingConstructor]
    public PropertyUsedByMetadataScanner(AnalyzerScope analyzerScope):base(analyzerScope)
    {
    }

    protected override bool IsSameMember(IMember analyzedMethod, IMember m)
    {
        var isSameMember = base.IsSameMember(analyzedMethod, m);
        if (isSameMember)
        {
            return true;
        }
        
        if (analyzedMethod is IProperty property)
        {
            if (property.Setter != null)
            {
                var isSameSetter = m.MetadataToken == property.Setter.MetadataToken
                                   && m.ParentModule.PEFile == property.ParentModule.PEFile;

                if (isSameSetter)
                {
                    return true;
                }
            }

            if (property.CanGet)
            {
                var isSameGetter = m.MetadataToken == property.Getter.MetadataToken
                                   && m.ParentModule.PEFile == property.ParentModule.PEFile;

                if (isSameGetter)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}