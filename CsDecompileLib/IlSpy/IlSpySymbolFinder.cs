using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using Accessibility = ICSharpCode.Decompiler.TypeSystem.Accessibility;
using ISymbol = ICSharpCode.Decompiler.TypeSystem.ISymbol;
using SymbolKind = ICSharpCode.Decompiler.TypeSystem.SymbolKind;
using TypeKind = ICSharpCode.Decompiler.TypeSystem.TypeKind;

namespace CsDecompileLib.IlSpy;

public class FakeType : ITypeDefinition
{
    private readonly TypeResolveResult _resolveResult;

    public FakeType(TypeResolveResult resolveResult)
    {
        _resolveResult = resolveResult;
    }

    public string FullName => _resolveResult.Type.FullName;

    public SymbolKind SymbolKind => SymbolKind.TypeDefinition;

    public IEnumerable<IAttribute> GetAttributes()
    {
        throw new NotImplementedException();
    }

    public bool HasAttribute(KnownAttribute attribute)
    {
        throw new NotImplementedException();
    }

    public IAttribute GetAttribute(KnownAttribute attribute)
    {
        throw new NotImplementedException();
    }

    public EntityHandle MetadataToken => throw new NotImplementedException();

    public string Name => _resolveResult.Type.Name;

    public ITypeDefinition DeclaringTypeDefinition => throw new NotImplementedException();

    public string ReflectionName => throw new NotImplementedException();

    public string Namespace => _resolveResult.Type.Namespace;

    public bool Equals(IType other)
    {
        throw new NotImplementedException();
    }

    public IType ChangeNullability(Nullability newNullability)
    {
        throw new NotImplementedException();
    }

    public ITypeDefinition GetDefinition()
    {
        throw new NotImplementedException();
    }

    public ITypeDefinitionOrUnknown GetDefinitionOrUnknown()
    {
        throw new NotImplementedException();
    }

    public IType AcceptVisitor(TypeVisitor visitor)
    {
        throw new NotImplementedException();
    }

    public IType VisitChildren(TypeVisitor visitor)
    {
        throw new NotImplementedException();
    }

    public TypeParameterSubstitution GetSubstitution()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IType> GetNestedTypes(Predicate<ITypeDefinition> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IType> GetNestedTypes(IReadOnlyList<IType> typeArguments, Predicate<ITypeDefinition> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IMethod> GetConstructors(Predicate<IMethod> filter = null, GetMemberOptions options = GetMemberOptions.IgnoreInheritedMembers)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IMethod> GetMethods(Predicate<IMethod> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IMethod> GetMethods(IReadOnlyList<IType> typeArguments, Predicate<IMethod> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IProperty> GetProperties(Predicate<IProperty> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IField> GetFields(Predicate<IField> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEvent> GetEvents(Predicate<IEvent> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IMember> GetMembers(Predicate<IMember> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IMethod> GetAccessors(Predicate<IMethod> filter = null, GetMemberOptions options = GetMemberOptions.None)
    {
        throw new NotImplementedException();
    }

    public TypeKind Kind => throw new NotImplementedException();

    public bool? IsReferenceType => throw new NotImplementedException();

    public bool IsByRefLike => throw new NotImplementedException();

    public Nullability Nullability => throw new NotImplementedException();

    public IReadOnlyList<ITypeDefinition> NestedTypes => throw new NotImplementedException();

    public IReadOnlyList<IMember> Members => throw new NotImplementedException();

    public IEnumerable<IField> Fields => throw new NotImplementedException();

    public IEnumerable<IMethod> Methods => throw new NotImplementedException();

    public IEnumerable<IProperty> Properties => throw new NotImplementedException();

    public IEnumerable<IEvent> Events => throw new NotImplementedException();

    public KnownTypeCode KnownTypeCode => throw new NotImplementedException();

    public IType EnumUnderlyingType => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public string MetadataName => throw new NotImplementedException();

    public FullTypeName FullTypeName => throw new NotImplementedException();

    public IType DeclaringType => throw new NotImplementedException();

    public bool HasExtensionMethods => throw new NotImplementedException();

    public Nullability NullableContext => throw new NotImplementedException();

    public bool IsRecord => throw new NotImplementedException();

    public IModule ParentModule => null;

    public Accessibility Accessibility => throw new NotImplementedException();

    public bool IsStatic => throw new NotImplementedException();

    public bool IsAbstract => throw new NotImplementedException();

    public bool IsSealed => throw new NotImplementedException();

    public int TypeParameterCount => throw new NotImplementedException();

    public IReadOnlyList<ITypeParameter> TypeParameters => throw new NotImplementedException();

    public IReadOnlyList<IType> TypeArguments => throw new NotImplementedException();

    public IEnumerable<IType> DirectBaseTypes => throw new NotImplementedException();

    public ICompilation Compilation => throw new NotImplementedException();
}
public class IlSpySymbolFinder
{
    private readonly IDecompilerTypeSystemFactory _typeSystemFactory;

    public IlSpySymbolFinder(IDecompilerTypeSystemFactory typeSystemFactory)
    {
        _typeSystemFactory = typeSystemFactory;
    }
    
    public ITypeDefinition FindTypeDefinition(string projectAssemblyFilePath, string symbolFullName)
    {
        var typeSystem = _typeSystemFactory.GetTypeSystem(projectAssemblyFilePath);
        if (typeSystem == null)
        {
            return null;
        }
        var tempFile = typeSystem.FindType(new FullTypeName(symbolFullName)) as ITypeDefinition;

        if (tempFile == null)
        {
            var tempFile2 = typeSystem.GetAllTypeDefinitions()
                .Where(d => d.FullName == symbolFullName).FirstOrDefault();

            tempFile = tempFile2 as ITypeDefinition;
        }

        return tempFile;
    }
    
    public IMethod FindMethod(string projectAssemblyFilePath, IMethodSymbol roslynMethod)
    {
        var typeFullName = roslynMethod.ContainingType.GetSymbolName();
        var typeDefinitionSymbol = FindTypeDefinition(projectAssemblyFilePath, typeFullName);
        var method = FindMethod(typeDefinitionSymbol, roslynMethod);
        return method;
    }
    
    public IField FindField(string projectAssemblyFilePath, IFieldSymbol roslynMethod)
    {
        var typeFullName = roslynMethod.ContainingType.GetSymbolName();
        var typeDefinitionSymbol = FindTypeDefinition(projectAssemblyFilePath, typeFullName);
        var result = FindField(typeDefinitionSymbol, roslynMethod);
        return result;
    }
    
    public IProperty FindProperty(string projectAssemblyFilePath, string typeName, string propertyName)
    {
        var typeDefinitionSymbol = FindTypeDefinition(projectAssemblyFilePath, typeName);
        var property = FindProperty(typeDefinitionSymbol, propertyName);
        return property;
    }
    
    public IEvent FindEvent(string projectAssemblyFilePath, string typeName, string eventName)
    {
        var typeDefinitionSymbol = FindTypeDefinition(projectAssemblyFilePath, typeName);
        var eventSymbol = FindEvent(typeDefinitionSymbol, eventName);
        return eventSymbol;
    }
    
    public ISymbol FindSymbolFromNode(AstNode node)
    {
        var symbolAtLocation = node.GetSymbol();

        while (symbolAtLocation == null && node.Parent != null)
        {
            node = node.Parent;
            symbolAtLocation = node?.GetSymbol();
            var resolveResult = node?.GetResolveResult();
            if (symbolAtLocation == null && resolveResult is TypeResolveResult typeResolveResult)
            {
                symbolAtLocation = new FakeType(typeResolveResult);
            }
        }

        return symbolAtLocation;
    }

    public ITypeDefinition FindParentType(AstNode node)
    {
        if (node == null)
        {
            return null;
        }
        if (node.NodeType == NodeType.TypeDeclaration)
        {
            var symbol = node.GetSymbol();
            var result = symbol as ITypeDefinition;
            return result;
        }

        var fromChild = FindParentType(node.Parent);
        return fromChild;
    }

    public AstNode GetNodeAt(AstNode node, int startLine, int startColumn)
    {
        foreach (var child in node.Children)
        {
            if (child.StartLocation.Line == startLine && child.StartLocation.Column == startColumn)
            {
                //We need to keep going down the tree to see if there is a more specific symbol, this could be an expression and we want the specific symbol
                //Ideally start sending end location line and column
                //This is inefficent we can stop recursing the rest of the tree here.  Just haven't figured that out yet
                if (!child.Children.Any(c =>
                        c.StartLocation.Line == startLine && c.StartLocation.Column == startColumn))
                {
                    return child;
                }
            }

            if (child.HasChildren)
            {
                var result = GetNodeAt(child, startLine, startColumn);
                if (result != null)
                {
                    return result;
                }
            }
        }
        return null;
    }
    
    private IMethod FindMethod(ITypeDefinition type, IMethodSymbol roslynMethod)
    {
        foreach (var member in type.Members)
        {
            if (member is IMethod ilSpyMethod)
            {
                if (RoslynToIlSpyEqualityExtensions.AreSameMethod(roslynMethod, ilSpyMethod))
                {
                    return ilSpyMethod;
                }

                if (roslynMethod != null)
                {
                    if (RoslynToIlSpyEqualityExtensions.AreSameMethod(roslynMethod.OriginalDefinition, ilSpyMethod))
                    {
                        return ilSpyMethod;
                    }
                }
                //Fallback for checking extension methods.  Roslyn seems to add a higher level method symbol without
                //The this parameter
                if (roslynMethod.IsExtensionMethod)
                {
                    if (roslynMethod.ReducedFrom != null)
                    {
                        if (RoslynToIlSpyEqualityExtensions.AreSameMethod(roslynMethod.ReducedFrom, ilSpyMethod))
                        {
                            return ilSpyMethod;
                        }
                    }
                }
            }
        }

        return null;
    }
    
    private IField FindField(ITypeDefinition type, IFieldSymbol roslynField)
    {
        foreach (var member in type.Members)
        {
            if (member is IField ilSpySymbol)
            {
                if (RoslynToIlSpyEqualityExtensions.AreMemberSymbol(roslynField, ilSpySymbol))
                {
                    return ilSpySymbol;
                }
            }
        }

        return null;
    }
    
    private static IProperty FindProperty(IType type, string methodName)
    {
        var properties = type.GetProperties().Where(m =>
        {
            if (m.FullName != methodName)
            {
                return false;
            }

            return true;
        });

        return properties.FirstOrDefault();
    }
    
    private static IEvent FindEvent(IType type, string eventName)
    {
        var properties = type.GetEvents().Where(m =>
        {
            if (m.FullName != eventName)
            {
                return false;
            }

            return true;
        });

        return properties.FirstOrDefault();
    }
}