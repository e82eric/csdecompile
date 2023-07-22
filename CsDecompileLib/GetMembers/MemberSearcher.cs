using System.Collections.Generic;
using System.Reflection.Metadata;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;

namespace CsDecompileLib.GetMembers;

public class MemberSearcher
{
    private readonly IDecompileWorkspace _workspace;

    public MemberSearcher(IDecompileWorkspace workspace)
    {
        _workspace = workspace;
    }

    public IEnumerable<(string assemblyFilePath, string containingTypeFullName, string memberName)> SearchForMembers(IEnumerable<string> assemblySearchStrings, string memberSearchString)
    {
        var peFiles = _workspace.GetAssemblies();
        var result = new List<(string, string, string)>();
        foreach (var peFile in peFiles)
        {
            SearchAssembly(
                peFile,
                assemblySearchStrings,
                memberSearchString,
                result);
        }

        return result;
    }

    private void SearchAssembly(
        PEFile peFile,
        IEnumerable<string> assemblySearchStrings,
        string memberSearchString,
        IList<(string, string, string)> found)
    {

        foreach (var assemblySearchString in assemblySearchStrings)
        {
            if (peFile.FullName.Contains(assemblySearchString))
            {
                AddMembers(peFile, memberSearchString, found);
                break;
            }
        }
    }

    private void AddMembers(
        PEFile peFile,
        string memberSearchName,
        IList<(string, string, string)> result)
    {
        var typeDefinitions = peFile.Metadata.TypeDefinitions;
        foreach (var typeDefinitionHandle in typeDefinitions)
        {
            var typeDef = peFile.Metadata.GetTypeDefinition(typeDefinitionHandle);
            foreach (var method in typeDef.GetMethods())
            {
                var methodDefinition = peFile.Metadata.GetMethodDefinition(method);
                AddMember(peFile, memberSearchName, result, methodDefinition.Name, typeDefinitionHandle);
            }

            foreach (var method in typeDef.GetEvents())
            {
                var methodDefinition = peFile.Metadata.GetEventDefinition(method);
                AddMember(peFile, memberSearchName, result, methodDefinition.Name, typeDefinitionHandle);
            }

            foreach (var method in typeDef.GetProperties())
            {
                var methodDefinition = peFile.Metadata.GetPropertyDefinition(method);
                AddMember(peFile, memberSearchName, result, methodDefinition.Name, typeDefinitionHandle);
            }

            foreach (var method in typeDef.GetFields())
            {
                var methodDefinition = peFile.Metadata.GetFieldDefinition(method);
                AddMember(peFile, memberSearchName, result, methodDefinition.Name, typeDefinitionHandle);
            }
        }
    }

    private static void AddMember(
        PEFile peFile,
        string memberSearchName,
        IList<(string, string, string)> result,
        StringHandle methodDefinition,
        TypeDefinitionHandle typeDefinitionHandle)
    {
        var memberName = peFile.Metadata.GetString(methodDefinition);

        if (memberName.Contains(memberSearchName))
        {
            var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile.Metadata);
            var fullName = fullTypeName.ReflectionName;
            result.Add((peFile.FileName, fullName, memberName));
        }
    }
}