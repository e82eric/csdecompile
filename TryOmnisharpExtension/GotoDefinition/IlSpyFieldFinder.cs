using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class IlSpyFieldFinder
{
    private readonly FieldInTypeFinder _fieldInTypeFinder;

    [ImportingConstructor]
    public IlSpyFieldFinder(FieldInTypeFinder fieldInTypeFinder)
    {
        _fieldInTypeFinder = fieldInTypeFinder;
    } 
    
    public (IlSpyMetadataSource2, string sourceText) Run(IField field)
    {
        var rootType = SymbolHelper.FindContainingType(field.DeclaringTypeDefinition);

        var (foundUse, sourceText) = _fieldInTypeFinder.Find(field.MetadataToken, rootType);

        var metadataSource = new IlSpyMetadataSource2
        {
            AssemblyName = rootType.ParentModule.AssemblyName,
            Column = foundUse.StartLocation.Column,
            Line = foundUse.StartLocation.Line,
            SourceText = $"{field.DeclaringType.ReflectionName} {foundUse.Statement.ToString().Replace("\r\n", "")}",
            StartColumn = foundUse.StartLocation.Column,
            EndColumn = foundUse.EndLocation.Column,
            ContainingTypeFullName = rootType.ReflectionName,
        };

        return (metadataSource, sourceText);
    }
}