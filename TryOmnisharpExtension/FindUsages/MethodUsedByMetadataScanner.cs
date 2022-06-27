using System.Reflection.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;
using GenericContext = ICSharpCode.Decompiler.TypeSystem.GenericContext;

namespace TryOmnisharpExtension.FindUsages;

public class MethodUsedByMetadataScanner : MemberUsedByMetadataScanner
{
    public MethodUsedByMetadataScanner(AnalyzerScope analyzerScope) : base(analyzerScope)
    {
    }

    protected override IMember GetMember(MetadataModule mainModule, EntityHandle member, GenericContext genericContext)
    {
        var result = (mainModule.ResolveEntity(member, genericContext) as IMember)?.MemberDefinition;
        return result;
    }
}