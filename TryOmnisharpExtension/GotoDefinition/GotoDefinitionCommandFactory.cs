using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    [Export(typeof(ICommandFactory<IGotoDefinitionCommand>))]
    public class GotoDefinitionCommandFactory : ICommandFactory<IGotoDefinitionCommand>
    {
        private readonly IlSpyTypeFinder _typeFinder;
        private readonly IlSpyMemberFinder _memberFinder;
        private readonly IlSpyPropertyFinder _propertyFinder;
        private readonly IlSpyEventFinder _eventFinder;

        [ImportingConstructor]
        public GotoDefinitionCommandFactory(
            IlSpyTypeFinder typeFinder,
            IlSpyMemberFinder memberFinder,
            IlSpyPropertyFinder propertyFinder,
            IlSpyEventFinder eventFinder)
        {
            _propertyFinder = propertyFinder;
            _eventFinder = eventFinder;
            _memberFinder = memberFinder;
            _typeFinder = typeFinder;
        }

        public IGotoDefinitionCommand GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
        {
            return new RosylynGotoDefinitionCommand(roslynSymbol);
        }

        public IGotoDefinitionCommand GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
        {
            var result = new GotoEventDefintionCommand(eventSymbol,_eventFinder, projectAssemblyFilePath);
            return result;
        }

        public IGotoDefinitionCommand GetForType(ITypeDefinition typeDefinition, string assemblyFilePath)
        {
            var result = new GotoTypeDefintionCommand(
                typeDefinition,
                _typeFinder,
                assemblyFilePath);
            return result;
        }

        public IGotoDefinitionCommand GetForMethod(IMethod method, string assemblyFilePath)
        {
            var result = new GotoMethodDefintionCommand(method, _memberFinder, assemblyFilePath);
            return result;
        }

        public IGotoDefinitionCommand GetForProperty(IProperty property, string assemblyFilePath)
        {
            var result = new GotoPropertyDefintionCommand(property,_propertyFinder, assemblyFilePath);
            return result;
        }
    }
}