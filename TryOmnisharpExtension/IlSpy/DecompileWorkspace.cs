using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.Metadata;
using AssemblyReference = ICSharpCode.Decompiler.Metadata.AssemblyReference;

namespace TryOmnisharpExtension.IlSpy
{
    [Shared]
    [Export(typeof(IDecompileWorkspace))]
    public class DecompileWorkspace : IDecompileWorkspace
    {
        private readonly Dictionary<string, PEFile> _byFilename = new(StringComparer.OrdinalIgnoreCase);
        private readonly IOmnisharpWorkspace _workspace;
        private readonly AssemblyResolverFactory _assemblyResolverFactory;

        [ImportingConstructor]
        public DecompileWorkspace(IOmnisharpWorkspace workspace, AssemblyResolverFactory assemblyResolverFactory)
        {
            _workspace = workspace;
            _assemblyResolverFactory = assemblyResolverFactory;
        }

        public void LoadDlls()
        {
            var projectAssemblyPaths = _workspace.GetProjectAssemblyPaths();
            foreach (var path in projectAssemblyPaths)
            {
                var projectDllFile = new FileInfo(path);
                var projectBinDir = projectDllFile.Directory;
                var binDirDlls = projectBinDir.GetFiles("*.dll", SearchOption.AllDirectories);

                foreach (var dllFilePath in binDirDlls)
                {
                    var dllPEFile = OpenAssembly(dllFilePath.FullName);
                    if (dllFilePath != null && dllPEFile != null)
                    {
                        var uniqueness = dllPEFile.FullName + "|" + dllPEFile.Metadata.MetadataVersion;
                        if (!_byFilename.ContainsKey(uniqueness))
                        {
                            _byFilename.Add(uniqueness, dllPEFile);
                        }
                    }
                }
            }
        }

		public PEFile[] GetAssemblies()
        {
            if (!_byFilename.Any())
            {
                LoadDlls();
            }
            return _byFilename.Values.ToArray();
        }

        private PEFile OpenAssembly(string file)
        {
            try
            {
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var result = LoadAssembly(fileStream, PEStreamOptions.PrefetchEntireImage, file);
                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        private PEFile LoadAssembly(Stream stream, PEStreamOptions streamOptions, string fileName)
        {
            var options = MetadataReaderOptions.ApplyWindowsRuntimeProjections;

            PEFile module = new PEFile(fileName, stream, streamOptions, metadataOptions: options);

            return module;
        }
    }
}