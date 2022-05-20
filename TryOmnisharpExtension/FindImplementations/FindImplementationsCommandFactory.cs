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
        private readonly OmniSharpWorkspace _omniSharpWorkspace;

        [ImportingConstructor]
        public FindImplementationsCommandFactory(
            IlSpyBaseTypeUsageFinder2 typeFinder,
            IlSpyMethodImplementationFinder methodImplementationFinder,
            OmniSharpWorkspace omniSharpWorkspace)
        {
            _typeFinder = typeFinder;
            _methodImplementationFinder = methodImplementationFinder;
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

        public INavigationCommand<FindImplementationsResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
        {
            throw new System.NotImplementedException();
        }

        public INavigationCommand<FindImplementationsResponse> GetForInSource(ISymbol roslynSymbol)
        {
            var result = new RosylynFindImplementationsCommand(roslynSymbol, _omniSharpWorkspace);
            return result;
        }
    }
}