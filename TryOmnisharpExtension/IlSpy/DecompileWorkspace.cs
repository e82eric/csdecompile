using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy
{
    [Shared]
    [Export(typeof(IDecompileWorkspace))]
    public class DecompileWorkspace : IDecompileWorkspace
    {
        private readonly Dictionary<string, PEFile> _byFilename = new(StringComparer.OrdinalIgnoreCase);
        private readonly Task _loadTask;

        [ImportingConstructor]
        public DecompileWorkspace(IOmnisharpWorkspace workspace, IDecompilerTypeSystemFactory typeSystemFactory)
        {
            var projectAssemblyPaths = workspace.GetProjectAssemblyPaths();
            _loadTask = LoadProjects(projectAssemblyPaths, typeSystemFactory);
        }

        private async Task LoadProjects(IEnumerable<string> projectAssemblyPaths, IDecompilerTypeSystemFactory typeSystemFactory)
        {
            foreach (var path in projectAssemblyPaths)
            {
                var projectPeFile = await OpenAssembly(path);
                _byFilename.Add(path, projectPeFile);
            }
        }

		public async Task<PEFile[]> GetAssemblies()
        {
            await _loadTask;
            return _byFilename.Values.ToArray();
        }

        private async Task<PEFile> OpenAssembly(string file, bool isAutoLoaded = false)
        {
            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                var result = LoadAssembly(fileStream, PEStreamOptions.PrefetchEntireImage, file);
                return result;
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