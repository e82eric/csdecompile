using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy
{
    [Export]
    public class IlSpyPropertyFinder
    {
        private readonly PropertyInTypeFinder2 _propertyInTypeFinder;

        [ImportingConstructor]
        public IlSpyPropertyFinder(
            PropertyInTypeFinder2 propertyInTypeFinder)
        {
            _propertyInTypeFinder = propertyInTypeFinder;
        }
        
        public (IlSpyMetadataSource2, string) Run(IProperty property)
        {
            var rootType = SymbolHelper.FindContainingType(property.DeclaringTypeDefinition);

            var (foundUse, sourceText) = _propertyInTypeFinder.Find(
                property.Setter?.MetadataToken,
                property.Getter?.MetadataToken,
                rootType);

            var metadataSource = new IlSpyMetadataSource2()
            {
                AssemblyName = rootType.ParentModule.AssemblyName,
                Column = foundUse.StartLocation.Column,
                Line = foundUse.StartLocation.Line,
                SourceText = $"{property.DeclaringType.ReflectionName} {foundUse.Statement.ToString().Replace("\r\n", "")}",
                StartColumn = foundUse.StartLocation.Column,
                EndColumn = foundUse.EndLocation.Column,
                ContainingTypeFullName = rootType.ReflectionName,
            };

            return (metadataSource, sourceText);
        }
    }
}