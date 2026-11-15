using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using StdIoHost.ProjectSystemExtraction;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;

namespace StdIoHost.SimpleProjectSystem;

internal class SimpleDecompileWorkspace : ICsDecompileWorkspace
{
    private readonly StdioEventEmitter _eventEmitter;
    private readonly MSBuildWorkspace _workspace;

    public SimpleDecompileWorkspace(StdioEventEmitter eventEmitter)
    {
        _eventEmitter = eventEmitter;

        _workspace = MSBuildWorkspace.Create();
        
        _workspace.WorkspaceFailed += (sender, args) =>
        {
            Console.WriteLine(args.Diagnostic.Message);
        };
    }
    
    public async Task Start(FileInfo solutionFileInfo)
    {
        try
        {
            var solution = await _workspace.OpenSolutionAsync(solutionFileInfo.FullName);
            
            _eventEmitter.Emit("SOLUTION_PARSED", new
            {
                NumberOfProjects = solution.Projects.Count(),
                SolutionName = solutionFileInfo.Name
            });

            foreach (var project in solution.Projects)
            {
                _eventEmitter.Emit("PROJECT_LOADED", new { ProjectName = project.Name });
            }
        }
        catch (Exception)
        {
            //TODO: Log Something
            _eventEmitter.Emit("PROJECT_FAILED", new { ProjectName = solutionFileInfo.Name });
            throw;
        }
    }

    public IEnumerable<string> GetProjectAssemblyPaths()
    {
        var result = _workspace.CurrentSolution.Projects.Select(p => p.OutputFilePath);
        return result;
    }

    public Solution CurrentSolution => _workspace.CurrentSolution;

    public Document GetDocument(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;

        var documentId = GetDocumentId(fileName);
        if (documentId == null)
        {
            return GetSourceGeneratedDocument(fileName);
        }

        return CurrentSolution.GetDocument(documentId);
    }

    // A request coming from a file that GeneratedSourceFiles wrote for a source generated document
    private Document GetSourceGeneratedDocument(string fileName)
    {
        if (!GeneratedSourceFiles.IsWrittenFile(fileName))
        {
            return null;
        }

        var writtenFilePath = NormalizePath(fileName);
        foreach (var project in CurrentSolution.Projects)
        {
            var generatedDocuments = project.GetSourceGeneratedDocumentsAsync().AsTask().GetAwaiter().GetResult();
            foreach (var generatedDocument in generatedDocuments)
            {
                if (generatedDocument.FilePath != null &&
                    string.Equals(
                        GeneratedSourceFiles.GetWrittenFilePath(generatedDocument.FilePath),
                        writtenFilePath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return generatedDocument;
                }
            }
        }

        return null;
    }
    
    // Editors can send forward slashes (Neovim with shellslash); Roslyn stores the paths MSBuild produced.
    private static string NormalizePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return filePath;
        try
        {
            return Path.GetFullPath(filePath);
        }
        catch (Exception)
        {
            return filePath;
        }
    }

    public DocumentId GetDocumentId(string filePath)
    {
        var documentIds = CurrentSolution.GetDocumentIdsWithFilePath(NormalizePath(filePath));
        return documentIds.FirstOrDefault();
    }

    public IEnumerable<Document> GetDocuments(string filePath)
    {
        var result = CurrentSolution
            .GetDocumentIdsWithFilePath(NormalizePath(filePath))
            .Select(id => CurrentSolution.GetDocument(id))
            .OfType<Document>();
        return result;
    }
}
