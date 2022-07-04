﻿using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class GotoDefinitionCommandFactory : ICommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>>
    {
        private readonly IlSpyTypeFinder _typeFinder;
        private readonly IlSpyDefinitionFinderBase<IMethod> _memberFinder;
        private readonly IlSpyPropertyFinder _propertyFinder;
        private readonly IlSpyDefinitionFinderBase<IEvent> _eventFinder;
        private readonly IlSpyDefinitionFinderBase<IField> _fieldFinder;

        public GotoDefinitionCommandFactory(
            IlSpyTypeFinder typeFinder,
            IlSpyDefinitionFinderBase<IMethod> memberFinder,
            IlSpyPropertyFinder propertyFinder,
            IlSpyDefinitionFinderBase<IEvent> eventFinder,
            IlSpyDefinitionFinderBase<IField> fieldFinder)
        {
            _fieldFinder = fieldFinder;
            _propertyFinder = propertyFinder;
            _eventFinder = eventFinder;
            _memberFinder = memberFinder;
            _typeFinder = typeFinder;
        }

        public INavigationCommand<DecompileGotoDefinitionResponse> GetForFileNotFound(string filePath)
        {
            var result = new FileNotFoundCommand(filePath);
            return result;
        }
        
        public INavigationCommand<DecompileGotoDefinitionResponse> SymbolNotFoundAtLocation(string filePath, int line, int column)
        {
            var result = new SymbolNotFoundAtLocationCommand(filePath, line, column);
            return result;
        }
        public INavigationCommand<DecompileGotoDefinitionResponse> GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
        {
            return new RosylynGotoDefinitionCommand(roslynSymbol);
        }

        public INavigationCommand<DecompileGotoDefinitionResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
        {
            var result = new GoToDefintionCommand<IEvent>(eventSymbol, _eventFinder, projectAssemblyFilePath);
            return result;
        }

        public INavigationCommand<DecompileGotoDefinitionResponse> GetForType(ITypeDefinition typeDefinition, string assemblyFilePath)
        {
            var result = new GoToDefintionCommand<ITypeDefinition>(typeDefinition, _typeFinder, assemblyFilePath);
            return result;
        }

        public INavigationCommand<DecompileGotoDefinitionResponse> GetForMethod(IMethod method, string assemblyFilePath)
        {
            var result = new GoToDefintionCommand<IMethod>(method, _memberFinder, assemblyFilePath);
            return result;
        }
        
        public INavigationCommand<DecompileGotoDefinitionResponse> GetForField(IField field, string assemblyFilePath)
        {
            var result = new GoToDefintionCommand<IField>(field, _fieldFinder, assemblyFilePath);
            return result;
        }

        public INavigationCommand<DecompileGotoDefinitionResponse> GetForProperty(IProperty property, string assemblyFilePath)
        {
            var result = new GoToDefintionCommand<IProperty>(property, _propertyFinder, assemblyFilePath);
            return result;
        }

        public INavigationCommand<DecompileGotoDefinitionResponse> GetForVariable(
            ILVariable variable,
            ITypeDefinition typeDefinition,
            SyntaxTree syntaxTree,
            string sourceText,
            string assemblyFilePath)
        {
            var variableInMethodBodyFinder = new VariableInTypeFinder();
            var ilSpyVariableDefintionFinder = new IlSpyVariableDefintionFinder(variableInMethodBodyFinder);
            var result = new GotoVariableDefintiionCommand(
                ilSpyVariableDefintionFinder,
                typeDefinition,
                syntaxTree,
                variable,
                sourceText,
                assemblyFilePath);
            return result;
        }
    }
}