using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;

namespace CsDecompileLib.GetMembers;

public static class PEMetadataHelper
{
    public static DecompileInfo MapTypeDefinitionToDecompileInfo(
        TypeDefinitionHandle typeDefinitionHandle,
        PEFile peFile)
    {
        var typeDef = peFile.Metadata.GetTypeDefinition(typeDefinitionHandle);
        var foundTypeName = peFile.Metadata.GetString(typeDef.Name);
        if (foundTypeName != null)
        {
            var fullTypeName = typeDefinitionHandle.GetFullTypeName(peFile.Metadata);
            var fullName = fullTypeName.ReflectionName;
            var shortName = fullTypeName.Name;

            var namespaceName = peFile.Metadata.GetString(typeDef.Namespace);
            var decompileInfo = new DecompileInfo
            {
                ParentAssemblyFilePath = peFile.FileName,
                AssemblyFilePath = peFile.FileName,
                AssemblyName = peFile.FullName,
                Column = 1,
                ContainingTypeFullName = fullName,
                ContainingTypeShortName = shortName,
                EndColumn = 1,
                Line = 1,
                SourceText = fullName,
                NamespaceName = namespaceName,
                StartColumn = 1,
                // UsageType = UsageTypes.Type
            };
            return decompileInfo;
        }

        return null;
    }
}