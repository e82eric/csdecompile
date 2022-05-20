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
                AssemblyFilePath = ilSpyMetadataSource.AssemblyFilePath
            };
            return decompileInfo;
        }
    }
}