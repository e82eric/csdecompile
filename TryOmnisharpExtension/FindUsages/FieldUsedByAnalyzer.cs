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
public class FieldUsedByAnalyzer
{
    private readonly AnalyzerScope _analyzerScope;
    const GetMemberOptions Options = GetMemberOptions.IgnoreInheritedMembers | GetMemberOptions.ReturnMemberDefinitions;

    [ImportingConstructor]
    public FieldUsedByAnalyzer(AnalyzerScope analyzerScope)
    {
        _analyzerScope = analyzerScope;
    }

    public async Task<IEnumerable<ISymbol>> Analyze(IField analyzedSymbol)
    {
        var mapping = GetCodeMappingInfo(analyzedSymbol.ParentModule.PEFile,
             analyzedSymbol.DeclaringTypeDefinition.MetadataToken);

        var result = new List<ISymbol>();
        var typesInScope = await _analyzerScope.GetTypesInScope(analyzedSymbol);
        foreach (var type in typesInScope)
        {
            var parentModule = (MetadataModule)type.ParentModule;
            mapping = GetCodeMappingInfo(parentModule.PEFile, type.MetadataToken);
            var methods = type.Members.OfType<IMethod>();
            foreach (var method in methods)
            {
                if (IsUsedInMethod(analyzedSymbol, method))
                {
                    var parent = mapping.GetParentMethod((MethodDefinitionHandle)method.MetadataToken);
                    var definition = parentModule.GetDefinition(parent);
                    result.Add(definition);
                }
            }

            foreach (var property in type.Properties)
            {
                if (property.CanGet && IsUsedInMethod(analyzedSymbol, property.Getter))
                {
                    result.Add(property);
                    continue;
                }

                if (property.CanSet && IsUsedInMethod(analyzedSymbol, property.Setter))
                {
                    result.Add(property);
                    continue;
                }
            }

            foreach (var @event in type.Events)
            {
                if (@event.CanAdd && IsUsedInMethod(analyzedSymbol, @event.AddAccessor))
                {
                    result.Add(@event);
                    continue;
                }

                if (@event.CanRemove && IsUsedInMethod(analyzedSymbol, @event.RemoveAccessor))
                {
                    result.Add(@event);
                    continue;
                }

                if (@event.CanInvoke && IsUsedInMethod(analyzedSymbol, @event.InvokeAccessor))
                {
                    result.Add(@event);
                    continue;
                }
            }
        }

        return result;
    }

    public virtual CodeMappingInfo GetCodeMappingInfo(PEFile module, System.Reflection.Metadata.EntityHandle member)
    {
        var declaringType = member.GetDeclaringType(module.Metadata);

        if (declaringType.IsNil && member.Kind == HandleKind.TypeDefinition)
        {
            declaringType = (TypeDefinitionHandle)member;
        }

        return new CodeMappingInfo(module, declaringType);
    }

    bool IsUsedInMethod(IField analyzedEntity, IMethod method)
    {
        return ScanMethodBody(analyzedEntity, method, method.GetMethodBody());
    }

    static bool ScanMethodBody(IField analyzedMethod, IMethod method, MethodBodyBlock methodBody)
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
                m = mainModule.ResolveEntity(member, genericContext) as IMember;
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
            case ILOpCode.Ldfld:
            case ILOpCode.Ldflda:
            case ILOpCode.Stfld:
            case ILOpCode.Ldsfld:
            case ILOpCode.Ldsflda:
            case ILOpCode.Stsfld:
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
        var isSameMember = m.MetadataToken == analyzedMethod.MetadataToken
                           && m.ParentModule.PEFile == analyzedMethod.ParentModule.PEFile;

        if (isSameMember)
        {
            return true;
        }

        var property = analyzedMethod as IProperty;
        if (property != null)
        {
            if (property.Setter != null)
            {
                var isSameSetter = m.MetadataToken == property.Setter.MetadataToken
                                   && m.ParentModule.PEFile == property.ParentModule.PEFile;

                if (isSameSetter)
                {
                    return true;
                }
            }

            var isSameGetter = m.MetadataToken == property.Getter.MetadataToken
                               && m.ParentModule.PEFile == property.ParentModule.PEFile;

            if (isSameGetter)
            {
                return true;
            }
        }

        return false;
    }
}