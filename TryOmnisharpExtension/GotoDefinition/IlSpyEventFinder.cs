using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy
{
    [Export]
    public class IlSpyEventFinder
    {
        private readonly EventInTypeFinder _eventInTypeFinder;

        [ImportingConstructor]
        public IlSpyEventFinder(EventInTypeFinder eventInTypeFinder)
        {
            _eventInTypeFinder = eventInTypeFinder;
        }
        
        public (IlSpyMetadataSource2, string) Find(IEvent eventSymbol)
        {
            var rootType = SymbolHelper.FindContainingType(eventSymbol.DeclaringTypeDefinition);

            var (foundUse, sourceText) = _eventInTypeFinder.Find(
                eventSymbol.MetadataToken,
                rootType);

            var metadataSource = new IlSpyMetadataSource2
            {
                AssemblyName = rootType.ParentModule.AssemblyName,
                Column = foundUse.StartLocation.Column,
                Line = foundUse.StartLocation.Line,
                SourceText = $"{eventSymbol.DeclaringType.ReflectionName} {foundUse.Statement.ToString().Replace("\r\n", "")}",
                StartColumn = foundUse.StartLocation.Column,
                EndColumn = foundUse.EndLocation.Column,
                ContainingTypeFullName = rootType.ReflectionName,
            };

            return (metadataSource, sourceText);
        }
    }
}