using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharp.Decompiler.IlSpy2;

[Export]
public class TypeUsedByAnalyzer2
{
    private readonly AnalyzerScope _analyzerScope;

    [ImportingConstructor]
    public TypeUsedByAnalyzer2(AnalyzerScope analyzerScope)
    {
        _analyzerScope = analyzerScope;
    }
    
    public async Task<IEnumerable<ISymbol>> Analyze(ISymbol analyzedSymbol)
    {
        var types = await _analyzerScope.GetTypesInScope((IEntity)analyzedSymbol);

        var result = new List<ISymbol>();
        foreach (var type in types)
        {
            foreach (var entity in ScanType((ITypeDefinition)analyzedSymbol, type))
            {
                result.Add(entity);
            }
        }

        return result;
    }

    IEnumerable<IEntity> ScanType(ITypeDefinition analyzedEntity, ITypeDefinition type)
    {
        if (analyzedEntity.ParentModule.PEFile == type.ParentModule.PEFile
            && analyzedEntity.MetadataToken == type.MetadataToken)
            yield break;

        var visitor = new TypeDefinitionUsedVisitor(analyzedEntity, topLevelOnly: false);

        foreach (var bt in type.DirectBaseTypes)
        {
            if (bt.FullName == analyzedEntity.FullName)
            {
                yield return type;
            }
            
            //Skipping PEFile file name comparison
            // bt.AcceptVisitor(visitor);
        }

        if (visitor.Found || ScanAttributes(visitor, type.GetAttributes()))
            yield return type;


        foreach (var member in type.Members)
        {
            visitor.Found = false;
            VisitMember(visitor, member, scanBodies: true);
            if (visitor.Found)
                yield return member;
        }
    }

    bool ScanAttributes(TypeDefinitionUsedVisitor visitor, IEnumerable<IAttribute> attributes)
    {
        foreach (var attribute in attributes)
        {
            foreach (var fa in attribute.FixedArguments)
            {
                CheckAttributeValue(fa.Value);
                if (visitor.Found)
                    return true;
            }

            foreach (var na in attribute.NamedArguments)
            {
                CheckAttributeValue(na.Value);
                if (visitor.Found)
                    return true;
            }
        }
        return false;

        void CheckAttributeValue(object value)
        {
            if (value is IType typeofType)
            {
                typeofType.AcceptVisitor(visitor);
            }
            else if (value is ImmutableArray<CustomAttributeTypedArgument<IType>> arr)
            {
                foreach (var element in arr)
                {
                    CheckAttributeValue(element.Value);
                }
            }
        }
    }

    void VisitMember(TypeDefinitionUsedVisitor visitor, IMember member, bool scanBodies = false)
    {
        member.DeclaringType.AcceptVisitor(visitor);
        switch (member)
        {
            case IField field:
                field.ReturnType.AcceptVisitor(visitor);

                if (!visitor.Found)
                    ScanAttributes(visitor, field.GetAttributes());
                break;
            case IMethod method:
                foreach (var p in method.Parameters)
                {
                    p.Type.AcceptVisitor(visitor);
                    if (!visitor.Found)
                        ScanAttributes(visitor, p.GetAttributes());
                }

                if (!visitor.Found)
                    ScanAttributes(visitor, method.GetAttributes());

                method.ReturnType.AcceptVisitor(visitor);

                if (!visitor.Found)
                    ScanAttributes(visitor, method.GetReturnTypeAttributes());

                foreach (var t in method.TypeArguments)
                {
                    t.AcceptVisitor(visitor);
                }

                foreach (var t in method.TypeParameters)
                {
                    t.AcceptVisitor(visitor);

                    if (!visitor.Found)
                        ScanAttributes(visitor, t.GetAttributes());
                }

                if (scanBodies && !visitor.Found)
                    ScanMethodBody(visitor, method, method.GetMethodBody());

                break;
            case IProperty property:
                foreach (var p in property.Parameters)
                {
                    p.Type.AcceptVisitor(visitor);
                }

                if (!visitor.Found)
                    ScanAttributes(visitor, property.GetAttributes());

                property.ReturnType.AcceptVisitor(visitor);

                if (scanBodies && !visitor.Found && property.CanGet)
                {
                    if (!visitor.Found)
                        ScanAttributes(visitor, property.Getter.GetAttributes());
                    if (!visitor.Found)
                        ScanAttributes(visitor, property.Getter.GetReturnTypeAttributes());

                    ScanMethodBody(visitor, property.Getter, property.Getter.GetMethodBody());
                }

                if (scanBodies && !visitor.Found && property.CanSet)
                {
                    if (!visitor.Found)
                        ScanAttributes(visitor, property.Setter.GetAttributes());
                    if (!visitor.Found)
                        ScanAttributes(visitor, property.Setter.GetReturnTypeAttributes());

                    ScanMethodBody(visitor, property.Setter, property.Setter.GetMethodBody());
                }

                break;
            case IEvent @event:
                @event.ReturnType.AcceptVisitor(visitor);

                if (scanBodies && !visitor.Found && @event.CanAdd)
                {
                    if (!visitor.Found)
                        ScanAttributes(visitor, @event.AddAccessor.GetAttributes());
                    if (!visitor.Found)
                        ScanAttributes(visitor, @event.AddAccessor.GetReturnTypeAttributes());

                    ScanMethodBody(visitor, @event.AddAccessor, @event.AddAccessor.GetMethodBody());
                }

                if (scanBodies && !visitor.Found && @event.CanRemove)
                {
                    if (!visitor.Found)
                        ScanAttributes(visitor, @event.RemoveAccessor.GetAttributes());
                    if (!visitor.Found)
                        ScanAttributes(visitor, @event.RemoveAccessor.GetReturnTypeAttributes());

                    ScanMethodBody(visitor, @event.RemoveAccessor, @event.RemoveAccessor.GetMethodBody());
                }

                if (scanBodies && !visitor.Found && @event.CanInvoke)
                {
                    if (!visitor.Found)
                        ScanAttributes(visitor, @event.InvokeAccessor.GetAttributes());
                    if (!visitor.Found)
                        ScanAttributes(visitor, @event.InvokeAccessor.GetReturnTypeAttributes());

                    ScanMethodBody(visitor, @event.InvokeAccessor, @event.InvokeAccessor.GetMethodBody());
                }

                break;
        }
    }

    void ScanMethodBody(TypeDefinitionUsedVisitor visitor, IMethod method, MethodBodyBlock methodBody)
    {
        if (methodBody == null)
            return;

        var module = (MetadataModule)method.ParentModule;
        var genericContext = new ICSharpCode.Decompiler.TypeSystem.GenericContext(); // type parameters don't matter for this analyzer

        if (!methodBody.LocalSignature.IsNil)
        {
            ImmutableArray<IType> localSignature;
            try
            {
                localSignature = module.DecodeLocalSignature(methodBody.LocalSignature, genericContext);
            }
            catch (BadImageFormatException)
            {
                // Issue #2197: ignore invalid local signatures
                localSignature = ImmutableArray<IType>.Empty;
            }
            foreach (var type in localSignature)
            {
                type.AcceptVisitor(visitor);

                if (visitor.Found)
                    return;
            }
        }

        var blob = methodBody.GetILReader();

        while (!visitor.Found && blob.RemainingBytes > 0)
        {
            var opCode = blob.DecodeOpCode();
            switch (opCode.GetOperandType())
            {
                case OperandType.Field:
                case OperandType.Method:
                case OperandType.Sig:
                case OperandType.Tok:
                case OperandType.Type:
                    var member = MetadataTokenHelpers.EntityHandleOrNil(blob.ReadInt32());
                    if (member.IsNil)
                        continue;
                    switch (member.Kind)
                    {
                        case HandleKind.TypeReference:
                        case HandleKind.TypeSpecification:
                        case HandleKind.TypeDefinition:
                            module.ResolveType(member, genericContext).AcceptVisitor(visitor);
                            if (visitor.Found)
                                return;
                            break;

                        case HandleKind.FieldDefinition:
                        case HandleKind.MethodDefinition:
                        case HandleKind.MemberReference:
                        case HandleKind.MethodSpecification:
                            VisitMember(visitor, module.ResolveEntity(member, genericContext) as IMember);

                            if (visitor.Found)
                                return;
                            break;

                        case HandleKind.StandaloneSignature:
                            var (_, fpt) = module.DecodeMethodSignature((StandaloneSignatureHandle)member, genericContext);
                            fpt.AcceptVisitor(visitor);

                            if (visitor.Found)
                                return;
                            break;

                        default:
                            break;
                    }
                    break;
                default:
                    blob.SkipOperand(opCode);
                    break;
            }
        }
    }
}
