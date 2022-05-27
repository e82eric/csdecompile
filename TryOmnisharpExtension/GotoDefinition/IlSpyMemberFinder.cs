using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class IlSpyMemberFinder
{
    private readonly MethodInTypeFinder2 _methodInTypeFinder;

    [ImportingConstructor]
    public IlSpyMemberFinder(MethodInTypeFinder2 methodInTypeFinder)
    {
        _methodInTypeFinder = methodInTypeFinder;
    } 
    
    public async Task<(IlSpyMetadataSource2, string sourceText)> Run(IMethod method)
    {
        var rootType = SymbolHelper.FindContainingType(method.DeclaringTypeDefinition);

        var (foundUse, sourceText) = await _methodInTypeFinder.Find(method.MetadataToken, rootType);

        var metadataSource = new IlSpyMetadataSource2
        {
            AssemblyName = rootType.ParentModule.AssemblyName,
            Column = foundUse.StartLocation.Column,
            Line = foundUse.StartLocation.Line,
            SourceText = $"{method.DeclaringType.ReflectionName} {foundUse.Statement.ToString().Replace("\r\n", "")}",
            StartColumn = foundUse.StartLocation.Column,
            EndColumn = foundUse.EndLocation.Column,
            ContainingTypeFullName = rootType.ReflectionName,
        };

        return (metadataSource, sourceText);
    }
}