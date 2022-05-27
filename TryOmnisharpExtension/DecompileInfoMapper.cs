using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    internal static class DecompileInfoMapper
    {
        public static DecompileInfo MapFromMetadataSource(IlSpyMetadataSource2 ilSpyMetadataSource)
        {
            var decompileInfo = new DecompileInfo
            {
                AssemblyName = ilSpyMetadataSource.AssemblyName,
                Line = ilSpyMetadataSource.Line,
                Column = ilSpyMetadataSource.Column,
                SourceText = ilSpyMetadataSource.SourceText,
                StartColumn = ilSpyMetadataSource.StartColumn,
                EndColumn = ilSpyMetadataSource.EndColumn,
                ContainingTypeFullName = ilSpyMetadataSource.ContainingTypeFullName,
                AssemblyFilePath = ilSpyMetadataSource.AssemblyFilePath,
                UsageType = ilSpyMetadataSource.UsageType,
                NamespaceName = ilSpyMetadataSource.NamespaceName,
                TypeName = ilSpyMetadataSource.TypeName,
                BaseTypeName = ilSpyMetadataSource.BaseTypeName,
                MethodName = ilSpyMetadataSource.MethodName,
                DotNetVersion = ilSpyMetadataSource.DotNetVersion,
                AssemblyVersion = ilSpyMetadataSource.AssemblyVersion,
                TypeFullName = ilSpyMetadataSource.TypeFullName
            };
            return decompileInfo;
        }
    }
}