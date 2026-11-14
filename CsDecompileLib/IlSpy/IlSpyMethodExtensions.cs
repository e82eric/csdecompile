using System;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

public static class IlSpyMethodExtensions
{
    public static MethodBodyBlock GetMethodBody(this IMethod method)
    {
        if (method.ParentModule == null || method.ParentModule.MetadataFile == null || !method.HasBody || method.MetadataToken.IsNil)
        {
            return null;
        }

        var module = method.ParentModule.MetadataFile;
        var md = module.Metadata.GetMethodDefinition((MethodDefinitionHandle)method.MetadataToken);
        try
        {
            return module.GetMethodBody(md.RelativeVirtualAddress);
        }
        catch (BadImageFormatException)
        {
            return null;
        }
    }
}
