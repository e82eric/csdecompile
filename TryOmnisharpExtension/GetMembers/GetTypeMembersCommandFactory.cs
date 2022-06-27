using System;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.GetMembers;

public class GetTypeMembersCommandFactory : ICommandFactory<INavigationCommand<GetTypeMembersResponse>>
{
    private readonly IlSpyTypeMembersFinder _typeFinder;

    public GetTypeMembersCommandFactory(
        IlSpyTypeMembersFinder typeFinder)
    {
        _typeFinder = typeFinder;
    }

    public INavigationCommand<GetTypeMembersResponse> GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
    {
        throw new NotImplementedException();
    }

    public INavigationCommand<GetTypeMembersResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        throw new NotImplementedException();
    }

    public INavigationCommand<GetTypeMembersResponse> GetForType(ITypeDefinition typeDefinition, string assemblyFilePath)
    {
        var result = new IlSpyTypeMembersCommand(typeDefinition, _typeFinder);
        return result;
    }

    public INavigationCommand<GetTypeMembersResponse> GetForMethod(IMethod method, string assemblyFilePath)
    {
        throw new NotImplementedException();
    }
        
    public INavigationCommand<GetTypeMembersResponse> GetForField(IField field, string assemblyFilePath)
    {
        throw new NotImplementedException();
    }

    public INavigationCommand<GetTypeMembersResponse> GetForProperty(IProperty property, string assemblyFilePath)
    {
        throw new NotImplementedException();
    }
}