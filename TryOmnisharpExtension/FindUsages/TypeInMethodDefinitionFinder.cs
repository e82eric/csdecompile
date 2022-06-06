using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class TypeInMethodDefinitionFinder
{
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public TypeInMethodDefinitionFinder(DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    public async Task<IEnumerable<UsageAsTextLocation>> Find(ITypeDefinition rootType, IMethod symbol, EntityHandle typeEntityHandle)
    {
        var fileName = symbol.Compilation.MainModule.PEFile.FileName;
        var cachingDecompiler = await _decompilerFactory.Get(fileName);
        var result = new List<UsageAsTextLocation>();

        var (syntaxTree, sourceText) = cachingDecompiler.Run(rootType);

        var methodNode = FindMethod(syntaxTree, symbol.MetadataToken);
        if (methodNode != null)
        {
            var lines = sourceText.Split(new []{"\r\n"}, StringSplitOptions.None);
            Find(methodNode, typeEntityHandle, result);
            foreach (var usageAsTextLocation in result)
            {
                var line = lines[usageAsTextLocation.StartLocation.Line - 1].Trim();
                usageAsTextLocation.Statement = line;
            }
        }

        return result;
    }

    private AstNode FindMethod(AstNode node, EntityHandle entityHandle)
    {
        if (node.NodeType == NodeType.Member)
        {
            var symbol = node.GetSymbol();
            var entity = symbol as IEntity;
            if (entity != null)
            {
                if (entity.MetadataToken == entityHandle)
                {
                    return node;
                }
            }
        }

        if (node.HasChildren)
        {
            foreach (var child in node.Children)
            {
                var method = FindMethod(child, entityHandle);
                if (method != null)
                {
                    return method;
                }
            }
        }

        return null;
    }

    private void Find(AstNode node, EntityHandle entityHandleToSearchFor, IList<UsageAsTextLocation> found)
    {
        var symbol = node.GetSymbol();

        if (node.Role == Roles.Type || node.Role == Roles.Parameter)
        {
            var resolveResult = node.Annotations.Where(a => a is ResolveResult).FirstOrDefault() as ResolveResult;
            if (resolveResult != null)
            {
                var resolveTypeDef = resolveResult.Type as ITypeDefinition;
                if (resolveTypeDef != null)
                {
                    if (resolveTypeDef.MetadataToken == entityHandleToSearchFor)
                    {
                        var entity = symbol as IEntity;
                        if (entity != null)
                        {
                            if (entity.MetadataToken == entityHandleToSearchFor)
                            {
                                var usage = new UsageAsTextLocation()
                                {
                                    TypeEntityHandle = entityHandleToSearchFor,
                                    StartLocation = node.StartLocation,
                                    EndLocation = node.EndLocation
                                };

                                found.Add(usage);
                            }
                        }
                    }
                }
            }
        }

        if (node.Role != Roles.Body)
        {
            foreach (var child in node.Children)
            {
                Find(child, entityHandleToSearchFor, found);
            }
        }
    }
}
