using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.GotoDefinition
{
    [Export(typeof(ICommandFactory<IGotoDefinitionCommand>))]
    public class GotoDefinitionCommandFactory : ICommandFactory<IGotoDefinitionCommand>
    {
        private readonly IlSpyTypeFinder _typeFinder;
        private readonly IlSpyMemberFinder _memberFinder;
        private readonly IlSpyPropertyFinder _propertyFinder;
        private readonly IlSpyEventFinder _eventFinder;
        private readonly IlSpyFieldFinder _fieldFinder;

        [ImportingConstructor]
        public GotoDefinitionCommandFactory(
            IlSpyTypeFinder typeFinder,
            IlSpyMemberFinder memberFinder,
            IlSpyPropertyFinder propertyFinder,
            IlSpyEventFinder eventFinder,
            IlSpyFieldFinder fieldFinder)
        {
            _fieldFinder = fieldFinder;
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
            var result = new GoToDefintionCommand<IEvent>(eventSymbol, _eventFinder, projectAssemblyFilePath);
            return result;
        }

        public IGotoDefinitionCommand GetForType(ITypeDefinition typeDefinition, string assemblyFilePath)
        {
            var result = new GoToDefintionCommand<ITypeDefinition>(typeDefinition, _typeFinder, assemblyFilePath);
            return result;
        }

        public IGotoDefinitionCommand GetForMethod(IMethod method, string assemblyFilePath)
        {
            var result = new GoToDefintionCommand<IMethod>(method, _memberFinder, assemblyFilePath);
            return result;
        }
        
        public IGotoDefinitionCommand GetForField(IField field, string assemblyFilePath)
        {
            var result = new GoToDefintionCommand<IField>(field, _fieldFinder, assemblyFilePath);
            return result;
        }

        public IGotoDefinitionCommand GetForProperty(IProperty property, string assemblyFilePath)
        {
            var result = new GoToDefintionCommand<IProperty>(property, _propertyFinder, assemblyFilePath);
            return result;
        }
    }
}