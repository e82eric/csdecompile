using System.Collections.Generic;
using CsDecompileLib.FindUsages;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.FindImplementations;

public class MemberOverrideInTypeFinder : IEntityUsedInTypeFinder<IMember>
{
    public IEnumerable<AstNode> Find(
        SyntaxTree syntaxTree,
        ITypeDefinition typeToSearchEntityHandle,
        IMember usageToFind)
    {
        var typeNode = syntaxTree.FindChildType(typeToSearchEntityHandle);
        var usageNodes = new List<AstNode>();
        if (typeNode != null)
        {
            FindUsages(typeNode, usageToFind, usageNodes);
        }

        return usageNodes;
    }
    
    private void FindUsages(AstNode node, IMember entityHandleToSearchFor, IList<AstNode> found)
    {
        //We are also going to stop recursion here since we don't need to check in parameters and method body
        if (node.Role == Roles.TypeMemberRole)
        {
            var symbol = node.GetSymbol();
            if (symbol is IMember entity)
            {
                //for now I guess we are ok with self matches being a "override"
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
        }
        else
        {
            foreach (var child in node.Children)
            {
                FindUsages(child, entityHandleToSearchFor, found);
            }
        }
    }
}