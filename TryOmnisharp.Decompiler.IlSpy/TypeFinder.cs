using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy.Analyzers;
using ICSharpCode.ILSpy.Analyzers.Builtin;

namespace IlSpy.Analyzer.Extraction;

public class TypenFinder
{
    public (UsageAsTextLocation, string) Find(ITypeDefinition symbol, EntityHandle handleToSearchFor, ITypeDefinition rootType)
    {
        var fileName = symbol.ParentModule.PEFile.FileName;
        var cachingDecompiler = DecompilerFactory.Get(fileName);
        (SyntaxTree syntaxTree, string source) = cachingDecompiler.Run2(rootType);

        var usage = Find(syntaxTree, handleToSearchFor);

        return (usage, source);
    }

    private UsageAsTextLocation Find(AstNode node, EntityHandle handleToSearchFor)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if(symbol is IEntity entity)
            {
                if ( handleToSearchFor == entity.MetadataToken && node.NodeType == NodeType.TypeDeclaration)
                {
                    var identifier = node.Children.Where(n =>
                    {
                        var entity = n as Identifier;
                        if (entity != null)
                        {
                            if (entity.Name == symbol.Name)
                            {
                                return true;
                            }
                        }

                        return false;
                    }).FirstOrDefault();

                    if (identifier != null)
                    {
                        var usage = new UsageAsTextLocation()
                        {
                            TypeEntityHandle = handleToSearchFor,
                            StartLocation = identifier.StartLocation,
                            EndLocation = identifier.EndLocation,
                            Statement = identifier.ToString()
                        };

                        return usage;
                    }
                }
            }
        }

        foreach (var child in node.Children)
        {
            var usage = Find(child, handleToSearchFor);
            if (usage != null)
            {
                return usage;
            }
        }

        return null;
    }
}