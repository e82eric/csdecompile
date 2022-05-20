using System.Collections.Generic;
using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    [Export(typeof(ICommandFactory<IGotoDefinitionCommand>))]
    public class GotoDefinitionCommandFactory : ICommandFactory<IGotoDefinitionCommand>
    {
        private readonly IlSpyTypeFinder2 _typeFinder;
        private readonly IlSpyMemberFinder2 _memberFinder;
        private readonly IlSpyPropertyFinder2 _propertyFinder;

        [ImportingConstructor]
        public GotoDefinitionCommandFactory(
            IlSpyTypeFinder2 typeFinder,
            IlSpyMemberFinder2 memberFinder,
            IlSpyPropertyFinder2 propertyFinder)
        {
            _propertyFinder = propertyFinder;
            _memberFinder = memberFinder;
            _typeFinder = typeFinder;
        }

        public IGotoDefinitionCommand GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
        {
            return new RosylynGotoDefinitionCommand(roslynSymbol);
        }

        public IGotoDefinitionCommand GetForType(ITypeDefinition typeDefinition, string assemblyFilePath)
        {
            return new GotoTypeDefintionCommand(
                typeDefinition,
                _typeFinder,
                assemblyFilePath);
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