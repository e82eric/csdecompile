using System.Collections.Generic;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

internal class IlSpyUsagesCommand<T, TResponse> : INavigationCommand<TResponse> where TResponse : LocationsResponse, new()
{
    private readonly T _symbol;
    private readonly IlSpyUsagesFinderBase<T> _usagesFinder;
        
    public IlSpyUsagesCommand(
        T symbol,
        IlSpyUsagesFinderBase<T> usagesFinder)
    {
        _symbol = symbol;
        _usagesFinder = usagesFinder;
    }
        
    public Task<ResponsePacket<TResponse>> Execute()
    {
        var metadataSources = _usagesFinder.Run(_symbol);

        var otherMembersToCheck = new List<IMember>();
        var body = new TResponse();

        foreach (var metadataSource in metadataSources)
        {
            body.Locations.Add(metadataSource);
        }

        if (_symbol is IMethod member)
        {
            foreach (var baseType in member.DeclaringTypeDefinition.DirectBaseTypes)
            {
                foreach (var baseTypeMember in baseType.GetMethods())
                {
                    var haveSameSignature = SymbolHelper.AreSameMethodSignature(member, baseTypeMember);
                    if (haveSameSignature)
                    {
                        otherMembersToCheck.Add(baseTypeMember);
                    }
                }
            }
            
            foreach (var interfaceMember in member.ExplicitlyImplementedInterfaceMembers)
            {
                otherMembersToCheck.Add(interfaceMember);
            }
        }
        else if (_symbol is IProperty property)
        {
            foreach (var baseType in property.DeclaringTypeDefinition.DirectBaseTypes)
            {
                foreach (var basePropertyMember in baseType.GetProperties())
                {
                    if (property.Name == basePropertyMember.Name)
                    {
                        otherMembersToCheck.Add(basePropertyMember);
                    }
                }
            }
        }
        else if (_symbol is IEvent evt)
        {
            foreach (var baseType in evt.DeclaringTypeDefinition.DirectBaseTypes)
            {
                foreach (var baseEvent in baseType.GetEvents())
                {
                    if (evt.Name == baseEvent.Name)
                    {
                        otherMembersToCheck.Add(baseEvent);
                    }
                }
            }
        }

        foreach (var eMember in otherMembersToCheck)
        {
            var interfaceMetadataSources = _usagesFinder.Run((T)eMember);

            foreach (var metadataSource in interfaceMetadataSources)
            {
                body.Locations.Add(metadataSource);
            }
        }
        
        var result = ResponsePacket.Ok(body);

        return Task.FromResult(result);
    }
}