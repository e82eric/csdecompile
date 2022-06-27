using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;
using GenericContext = ICSharpCode.Decompiler.TypeSystem.GenericContext;

namespace TryOmnisharpExtension.FindUsages;

public class MemberUsedByMetadataScanner : IMetadataUsagesScanner<IMember>
{
    private readonly AnalyzerScope _analyzerScope;
    const GetMemberOptions Options = GetMemberOptions.IgnoreInheritedMembers | GetMemberOptions.ReturnMemberDefinitions;

    public MemberUsedByMetadataScanner(AnalyzerScope analyzerScope)
    {
        _analyzerScope = analyzerScope;
    }

    public IEnumerable<ITypeDefinition> GetRootTypesThatUseSymbol(IMember analyzedSymbol)
    {
        var result = new List<ITypeDefinition>();
        var parentTypesAlreadyAdded = new HashSet<string>();
        var typesInScope = _analyzerScope.GetTypesInScope(analyzedSymbol);
        foreach (var type in typesInScope)
        {
            var parentType = SymbolHelper.FindContainingType(type);
            if (!parentTypesAlreadyAdded.Contains(parentType.FullName))
            {
                var typeUsesMethod = TypeUsesMethod(analyzedSymbol, type);
                if (typeUsesMethod)
                {
                    result.Add(parentType);
                    parentTypesAlreadyAdded.Add(parentType.FullName);
                }
            }
        }

        return result;
    }

    private bool TypeUsesMethod(IMember analyzedSymbol, ITypeDefinition type)
    {
        var isUsedInMethods = IsUsedInMethods(analyzedSymbol, type);
        if (isUsedInMethods)
        {
            return true;
        }

        var isUsedInProperties = IsUsedInProperties(analyzedSymbol, type);
        if (isUsedInProperties)
        {
            return true;
        }

        var isUsedInEvents = IsUsedInEvents(analyzedSymbol, type);
        if (isUsedInEvents)
        {
            return true;
        }

        return false;
    }

    private bool IsUsedInEvents(IMember analyzedSymbol, ITypeDefinition type)
    {
        foreach (var @event in type.Events)
        {
            if (@event.CanAdd)
            {
                var isUsedInMethod = IsUsedInMethod(analyzedSymbol, @event.AddAccessor);
                if (isUsedInMethod)
                {
                    return true;
                }
            }

            if (@event.CanRemove)
            {
                var usedInMethod = IsUsedInMethod(analyzedSymbol, @event.RemoveAccessor);
                if (usedInMethod)
                {
                    return true;
                }
            }

            if (@event.CanInvoke)
            {
                var inMethod = IsUsedInMethod(analyzedSymbol, @event.InvokeAccessor);
                if (inMethod)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsUsedInProperties(IMember analyzedSymbol, ITypeDefinition type)
    {
        foreach (var property in type.Properties)
        {
            if (property.CanGet)
            {
                var isUsedInMethod = IsUsedInMethod(analyzedSymbol, property.Getter);
                if (isUsedInMethod)
                {
                    return true;
                }
            }

            if (property.CanSet)
            {
                var setterUsedInMethod = IsUsedInMethod(analyzedSymbol, property.Setter);
                if (setterUsedInMethod)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsUsedInMethods(IMember analyzedSymbol, ITypeDefinition type)
    {
        var methods = type.GetMembers(m => m is IMethod, Options).OfType<IMethod>();
        foreach (var method in methods)
        {
            var isUsedInMethod = IsUsedInMethod(analyzedSymbol, method);
            if (isUsedInMethod)
            {
                return true;
            }
        }

        return false;
    }

    bool IsUsedInMethod(IMember analyzedEntity, IMethod method)
    {
        var result = ScanMethodBody(analyzedEntity, method, method.GetMethodBody());
        return result;
    }

    private bool ScanMethodBody(IMember analyzedMethod, IMethod method, MethodBodyBlock methodBody)
    {
        if (methodBody == null)
            return false;

        var mainModule = (MetadataModule)method.ParentModule;
        var blob = methodBody.GetILReader();

        var baseMethod = InheritanceHelper.GetBaseMember(analyzedMethod);
        var genericContext =
            new ICSharpCode.Decompiler.TypeSystem.GenericContext(); // type parameters don't matter for this analyzer

        while (blob.RemainingBytes > 0)
        {
            ILOpCode opCode;
            try
            {
                opCode = blob.DecodeOpCode();
                if (!IsSupportedOpCode(opCode))
                {
                    ILParser.SkipOperand(ref blob, opCode);
                    continue;
                }
            }
            catch (BadImageFormatException)
            {
                return false; // unexpected end of blob
            }

            var member = MetadataTokenHelpers.EntityHandleOrNil(blob.ReadInt32());
            if (member.IsNil || !member.Kind.IsMemberKind())
                continue;

            IMember m;
            try
            {
                // m = (mainModule.ResolveEntity(member, genericContext) as IMember)?.MemberDefinition;
                m = GetMember(mainModule, member, genericContext);
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            if (m == null)
            {
                continue;
            }

            if (analyzedMethod.IsOverridable)
            {
                var mBaseMembers = InheritanceHelper.GetBaseMembers(m, true);
                foreach (var mbaseMember in mBaseMembers)
                {
                    if (opCode == ILOpCode.Callvirt && baseMethod != null)
                    {
                        if (IsSameMember(baseMethod, mbaseMember))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (IsSameMember(analyzedMethod, mbaseMember))
                        {
                            return true;
                        }
                    }
                }
            }
            
            if (opCode == ILOpCode.Callvirt && baseMethod != null)
            {
                if (IsSameMember(baseMethod, m))
                {
                    return true;
                }
            }
            else
            {
                if (IsSameMember(analyzedMethod, m))
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected virtual IMember GetMember(MetadataModule mainModule, EntityHandle member, GenericContext genericContext)
    {
        var result = mainModule.ResolveEntity(member, genericContext) as IMember;
        return result;
    }

    protected virtual bool IsSupportedOpCode(ILOpCode opCode)
    {
        switch (opCode)
        {
            case ILOpCode.Call:
            case ILOpCode.Callvirt:
            case ILOpCode.Ldtoken:
            case ILOpCode.Ldftn:
            case ILOpCode.Ldvirtftn:
            case ILOpCode.Newobj:
                return true;
            default:
                return false;
        }
    }
    
    protected virtual bool IsSameMember(IMember analyzedMethod, IMember m)
    {
        var isSameMember = m.AreSameUsingToken(analyzedMethod);

        if (isSameMember)
        {
            return true;
        }

        return false;
    }
}