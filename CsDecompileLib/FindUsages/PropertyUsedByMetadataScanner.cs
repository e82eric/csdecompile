using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

public class PropertyUsedByMetadataScanner : MemberUsedByMetadataScanner
{
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
                var isSameSetter = m.AreSameUsingToken(property.Setter);

                if (isSameSetter)
                {
                    return true;
                }
            }

            if (property.CanGet)
            {
                var isSameGetter = m.AreSameUsingToken(property.Getter);

                if (isSameGetter)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}