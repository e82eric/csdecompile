using System;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.FindUsages;

public static class IlSpyMethodExtensions
{
    public static MethodBodyBlock GetMethodBody(this IMethod method)
    {
        if (!method.HasBody || method.MetadataToken.IsNil)
            return null;
        var module = method.ParentModule.PEFile;
        var md = module.Metadata.GetMethodDefinition((MethodDefinitionHandle)method.MetadataToken);
        try
        {
            return module.Reader.GetMethodBody(md.RelativeVirtualAddress);
        }
        catch (BadImageFormatException)
        {
            return null;
        }
    }
}
