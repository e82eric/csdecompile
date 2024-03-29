﻿using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.FindUsages;

public class MemberUsedInTypeFinder : IEntityUsedInTypeFinder<IMember>
{
    public IEnumerable<AstNode> Find(
        SyntaxTree syntaxTree,
        ITypeDefinition typeToSearchEntityHandle,
        IMember usageToFind)
    {
        var usageNodes = new List<AstNode>();
        var typeNode = syntaxTree.FindChildType(typeToSearchEntityHandle);
        if (typeNode != null)
        {
            FindUsages(typeNode, usageToFind, usageNodes);
        }

        return usageNodes;
    }
    
    private void FindUsages(AstNode node, IMember entityHandleToSearchFor, IList<AstNode> found)
    {
        //This recurses through the entire class and finds any nodes that reference the members token
        var symbol = node.GetSymbol();
        if (symbol is IMember entity)
        {
            if (entity.AreSameUsingToken(entityHandleToSearchFor))
            {
                found.Add(node);
            }
            else
            {
                //If we don't find a match and the member is overridable we need to check that the method isn't hidden in a base type
                if (entityHandleToSearchFor.IsOverridable)
                {
                    //Checking name first,  before doing all of the base type searching
                    if (entity.Name == entityHandleToSearchFor.Name)
                    {
                        var entityBaseMembers = InheritanceHelper.GetBaseMembers(entity, true);
                        foreach (var entityBaseMember in entityBaseMembers)
                        {
                            if (entityBaseMember.AreSameUsingToken(entityHandleToSearchFor))
                            {
                                found.Add(node);
                                break;
                            }
                        }
                    }
                }
            }
        }

        foreach (var child in node.Children)
        {
            FindUsages(child, entityHandleToSearchFor, found);
        }
    }
}