using System.Composition;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;
using GenericContext = ICSharpCode.Decompiler.TypeSystem.GenericContext;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class MethodUsedByMetadataScanner : MemberUsedByMetadataScanner
{
    [ImportingConstructor]
    public MethodUsedByMetadataScanner(AnalyzerScope analyzerScope) : base(analyzerScope)
    {
    }

    protected override IMember GetMember(MetadataModule mainModule, EntityHandle member, GenericContext genericContext)
    {
        var result = (mainModule.ResolveEntity(member, genericContext) as IMember)?.MemberDefinition;
        return result;
    }
}