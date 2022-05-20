using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class MethodUsedByAnalyzer
{
    private readonly AnalyzerScope _analyzerScope;
    const GetMemberOptions Options = GetMemberOptions.IgnoreInheritedMembers | GetMemberOptions.ReturnMemberDefinitions;

    [ImportingConstructor]
    public MethodUsedByAnalyzer(AnalyzerScope analyzerScope)
    {
        _analyzerScope = analyzerScope;
    }

    public async Task<IEnumerable<ISymbol>> Analyze(IMember analyzedSymbol)
    {
        var analyzedMethod = (IMember)analyzedSymbol;
        var mapping = GetCodeMappingInfo(
            analyzedMethod.ParentModule.PEFile,
            analyzedMethod.DeclaringTypeDefinition.MetadataToken);

        var result = new List<ISymbol>();
        foreach (var type in await _analyzerScope.GetTypesInScope(analyzedSymbol))
        {
            var parentModule = (MetadataModule)type.ParentModule;
            mapping = GetCodeMappingInfo(parentModule.PEFile, type.MetadataToken);
            var methods = type.GetMembers(m => m is IMethod, Options).OfType<IMethod>();
            foreach (var method in methods)
            {
                if (IsUsedInMethod((IMethod)analyzedSymbol, method))
                {
                    var parent = mapping.GetParentMethod((MethodDefinitionHandle)method.MetadataToken);
                    var definition = parentModule.GetDefinition(parent);
                    result.Add(definition);
                }
            }

            foreach (var property in type.Properties)
            {
                if (property.CanGet && IsUsedInMethod((IMethod)analyzedSymbol, property.Getter))
                {
                    result.Add(property);
                    continue;
                }

                if (property.CanSet && IsUsedInMethod((IMethod)analyzedSymbol, property.Setter))
                {
                    result.Add(property);
                    continue;
                }
            }

            foreach (var @event in type.Events)
            {
                if (@event.CanAdd && IsUsedInMethod((IMethod)analyzedSymbol, @event.AddAccessor))
                {
                    result.Add(@event);
                    continue;
                }

                if (@event.CanRemove && IsUsedInMethod((IMethod)analyzedSymbol, @event.RemoveAccessor))
                {
                    result.Add(@event);
                    continue;
                }

                if (@event.CanInvoke && IsUsedInMethod((IMethod)analyzedSymbol, @event.InvokeAccessor))
                {
                    result.Add(@event);
                    continue;
                }
            }
        }

        return result;
    }

    public virtual CodeMappingInfo GetCodeMappingInfo(PEFile module, EntityHandle member)
    {
        var declaringType = member.GetDeclaringType(module.Metadata);

        if (declaringType.IsNil && member.Kind == HandleKind.TypeDefinition)
        {
            declaringType = (TypeDefinitionHandle)member;
        }

        return new CodeMappingInfo(module, declaringType);
    }

    bool IsUsedInMethod(IMethod analyzedEntity, IMethod method)
    {
        return ScanMethodBody(analyzedEntity, method, method.GetMethodBody());
    }

    static bool ScanMethodBody(IMethod analyzedMethod, IMethod method, MethodBodyBlock methodBody)
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
                m = (mainModule.ResolveEntity(member, genericContext) as IMember)?.MemberDefinition;
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            if (m == null)
                continue;

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

    static bool IsSupportedOpCode(ILOpCode opCode)
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

    static bool IsSameMember(IMember analyzedMethod, IMember m)
    {
        if (m.MetadataToken == analyzedMethod.MetadataToken)
        {
            if(m.ParentModule.PEFile == analyzedMethod.ParentModule.PEFile)
            {
                return true;
            }
        }

        return false;

        // return m.MetadataToken == analyzedMethod.MetadataToken
        //        && m.ParentModule.PEFile == analyzedMethod.ParentModule.PEFile;
    }
}
