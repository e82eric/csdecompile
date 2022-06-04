using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using OmniSharp;
using TryOmnisharpExtension.IlSpy;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension
{
    [Export(typeof(ICommandFactory<INavigationCommand<FindImplementationsResponse>>))]
    public class FindImplementationsCommandFactory : ICommandFactory<INavigationCommand<FindImplementationsResponse>>
    {
        private readonly IlSpyBaseTypeUsageFinder2 _typeFinder;
        private readonly IlSpyMethodImplementationFinder _methodImplementationFinder;
        private readonly IlSpyPropertyImplementationFinder _propertyImplementationFinder;
        private readonly IlSpyEventImplementationFinder _eventImplementationFinder;
        private readonly OmniSharpWorkspace _omniSharpWorkspace;

        [ImportingConstructor]
        public FindImplementationsCommandFactory(
            IlSpyBaseTypeUsageFinder2 typeFinder,
            IlSpyMethodImplementationFinder methodImplementationFinder,
            IlSpyPropertyImplementationFinder propertyImplementationFinder,
            IlSpyEventImplementationFinder eventImplementationFinder,
            OmniSharpWorkspace omniSharpWorkspace)
        {
            _typeFinder = typeFinder;
            _methodImplementationFinder = methodImplementationFinder;
            _propertyImplementationFinder = propertyImplementationFinder;
            _eventImplementationFinder = eventImplementationFinder;
            _omniSharpWorkspace = omniSharpWorkspace;
        }

        public INavigationCommand<FindImplementationsResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
        {
            return new FindTypeImplementationsCommand(
                projectAssemblyFilePath,
                typeDefinition,
                _typeFinder);
        }

        public INavigationCommand<FindImplementationsResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
        {
            return new FindMethodImplementationsCommand(
                projectAssemblyFilePath,
                method,
                _methodImplementationFinder);
        }

        public INavigationCommand<FindImplementationsResponse> GetForField(IField field, string projectAssemblyFilePath)
        {
            throw new System.NotImplementedException();
        }

        public INavigationCommand<FindImplementationsResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
        {
            return new FindPropertyImplementationsCommand(
                projectAssemblyFilePath,
                property,
                _propertyImplementationFinder);
        }
        
        public INavigationCommand<FindImplementationsResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
        {
            return new FindEventImplementationsCommand(
                projectAssemblyFilePath,
                eventSymbol,
                _eventImplementationFinder);
        }

        public INavigationCommand<FindImplementationsResponse> GetForInSource(ISymbol roslynSymbol)
        {
            var result = new RosylynFindImplementationsCommand(roslynSymbol, _omniSharpWorkspace);
            return result;
        }
    }
}