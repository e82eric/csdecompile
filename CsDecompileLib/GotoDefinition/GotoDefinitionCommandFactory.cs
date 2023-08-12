using CsDecompileLib.FindUsages;
using CsDecompileLib.GetMembers;
using CsDecompileLib.IlSpy.Ast;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition
{
    public class GotoDefinitionCommandFactory : ICommandFactory<INavigationCommand<FindImplementationsResponse>>
    {
        private readonly IlSpyDefinitionFinderBase<ITypeDefinition> _typeFinder;
        private readonly IlSpyDefinitionFinderBase<IMethod> _memberFinder;
        private readonly IlSpyDefinitionFinderBase<IProperty> _propertyFinder;
        private readonly IlSpyDefinitionFinderBase<IEvent> _eventFinder;
        private readonly IlSpyDefinitionFinderBase<IField> _fieldFinder;
        private readonly IlSpyTypesInReferencesSearcher _ilSpyTypesRepository;
        private readonly RoslynAllTypesRepository _roslynAllTypesRepository;

        public GotoDefinitionCommandFactory(
            IlSpyDefinitionFinderBase<ITypeDefinition> typeFinder,
            IlSpyDefinitionFinderBase<IMethod> memberFinder,
            IlSpyDefinitionFinderBase<IProperty> propertyFinder,
            IlSpyDefinitionFinderBase<IEvent> eventFinder,
            IlSpyDefinitionFinderBase<IField> fieldFinder,
            IlSpyTypesInReferencesSearcher ilSpyTypesRepository,
            RoslynAllTypesRepository roslynAllTypesRepository)
        {
            _fieldFinder = fieldFinder;
            _ilSpyTypesRepository = ilSpyTypesRepository;
            _roslynAllTypesRepository = roslynAllTypesRepository;
            _propertyFinder = propertyFinder;
            _eventFinder = eventFinder;
            _memberFinder = memberFinder;
            _typeFinder = typeFinder;
        }

        public INavigationCommand<FindImplementationsResponse> GetForFileNotFound(string filePath)
        {
            var result = new FileNotFoundCommand<FindImplementationsResponse>(filePath);
            return result;
        }
        
        public INavigationCommand<FindImplementationsResponse> SymbolNotFoundAtLocation(string filePath, int line, int column)
        {
            var result = new SymbolNotFoundAtLocationCommand<FindImplementationsResponse>(filePath, line, column);
            return result;
        }
        public INavigationCommand<FindImplementationsResponse> GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
        {
            return new RoslynGotoDefinitionCommand(roslynSymbol);
        }

        public INavigationCommand<FindImplementationsResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
        {
            var result = new GoToDefinitionCommand<IEvent>(eventSymbol, _eventFinder, projectAssemblyFilePath);
            return result;
        }

        public INavigationCommand<FindImplementationsResponse> GetForType(ITypeDefinition typeDefinition, string assemblyFilePath)
        {
            var result = new GoToDefinitionCommand<ITypeDefinition>(typeDefinition, _typeFinder, assemblyFilePath);
            return result;
        }

        public INavigationCommand<FindImplementationsResponse> GetForMethod(IMethod method, string assemblyFilePath)
        {
            var result = new GoToDefinitionCommand<IMethod>(method, _memberFinder, assemblyFilePath);
            return result;
        }

        public INavigationCommand<FindImplementationsResponse> GetForNamespace(string namespaceString)
        {
            return new EverywhereGoToNamespaceDefinitionCommand(_ilSpyTypesRepository, _roslynAllTypesRepository, namespaceString);
        }

        public INavigationCommand<FindImplementationsResponse> GetForEnumField(IField field, string projectAssemblyFilePath)
        {
            throw new System.NotImplementedException();
        }

        public INavigationCommand<FindImplementationsResponse> GetForField(IField field, string assemblyFilePath)
        {
            var result = new GoToDefinitionCommand<IField>(field, _fieldFinder, assemblyFilePath);
            return result;
        }

        public INavigationCommand<FindImplementationsResponse> GetForProperty(IProperty property, string assemblyFilePath)
        {
            var result = new GoToDefinitionCommand<IProperty>(property, _propertyFinder, assemblyFilePath);
            return result;
        }

        public INavigationCommand<FindImplementationsResponse> GetForVariable(
            ILVariable variable,
            ITypeDefinition typeDefinition,
            AstNode methodNode,
            string sourceText,
            string assemblyFilePath)
        {
            var variableInMethodBodyFinder = new VariableNodeInTypeAstFinder();
            var ilSpyVariableDefintionFinder = new IlSpyVariableDefinitionFinder(variableInMethodBodyFinder);
            var result = new GotoVariableDefinitionCommand(
                ilSpyVariableDefintionFinder,
                typeDefinition,
                methodNode,
                variable,
                sourceText,
                assemblyFilePath);
            return result;
        }
    }
}