using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using TryOmnisharpExtension.IlSpy;

public class OmniSharpWorkspace2 : Workspace, IOmniSharpWorkspace
{
    private readonly ILogger<OmniSharpWorkspace2> _logger;

    private bool isInitialized;

    public OmniSharpWorkspace2(HostServicesAggregator aggregator, ILoggerFactory loggerFactory)
        : base(aggregator.CreateHostServices(), "Custom")
    {
        _logger = loggerFactory.CreateLogger<OmniSharpWorkspace2>();
    }

    public override bool CanOpenDocuments => true;
    
    public void AddProjectReference(Microsoft.CodeAnalysis.ProjectId projectId, ProjectReference projectReference)
    {
        OnProjectReferenceAdded(projectId, projectReference);
    }

    public void RemoveProjectReference(Microsoft.CodeAnalysis.ProjectId projectId, ProjectReference projectReference)
    {
        OnProjectReferenceRemoved(projectId, projectReference);
    }

    public void AddMetadataReference(Microsoft.CodeAnalysis.ProjectId projectId, MetadataReference metadataReference)
    {
        OnMetadataReferenceAdded(projectId, metadataReference);
    }

    public void RemoveMetadataReference(Microsoft.CodeAnalysis.ProjectId projectId, MetadataReference metadataReference)
    {
        OnMetadataReferenceRemoved(projectId, metadataReference);
    }

    public void UpdateCompilationOptionsForProject(Microsoft.CodeAnalysis.ProjectId projectId, CompilationOptions options)
    {
        OnCompilationOptionsChanged(projectId, options);
    }

    public void AddDocument(DocumentInfo documentInfo)
    {
        // if the file has already been added as a misc file,
        // because of a possible race condition between the updates of the project systems,
        // remove the misc file and add the document as required
        // TryRemoveMiscellaneousDocument(documentInfo.FilePath);

        OnDocumentAdded(documentInfo);
    }

    public DocumentId AddDocument(Project project, string filePath, SourceCodeKind sourceCodeKind = SourceCodeKind.Regular)
    {
        var documentId = DocumentId.CreateNewId(project.Id);
        return AddDocument(documentId, project, filePath, sourceCodeKind);
    }

    public DocumentId AddDocument(DocumentId documentId, Project project, string filePath, SourceCodeKind sourceCodeKind = SourceCodeKind.Regular)
    {
        return AddDocument(documentId, project, filePath, new OmniSharpTextLoader(filePath), sourceCodeKind);
    }

    internal DocumentId AddDocument(DocumentId documentId, Project project, string filePath, TextLoader loader, SourceCodeKind sourceCodeKind = SourceCodeKind.Regular)
    {
        var basePath = Path.GetDirectoryName(project.FilePath);
        var fullPath = Path.GetDirectoryName(filePath);
    
        IEnumerable<string> folders = null;
    
        // folder computation is best effort. in case of exceptions, we back out because it's not essential for core features
        try
        {
            // find the relative path from project file to our document
            var relativeDocumentPath = FileSystemHelper.GetRelativePath(fullPath, basePath);
    
            // only set document's folders if
            // 1. relative path was computed
            // 2. path is not pointing any level up
            if (relativeDocumentPath != null && !relativeDocumentPath.StartsWith(".."))
            {
                folders = relativeDocumentPath?.Split(new[] { Path.DirectorySeparatorChar });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"An error occurred when computing a relative path from {basePath} to {fullPath}. Document at {filePath} will be processed without folder structure.");
        }
    
        var documentInfo = DocumentInfo.Create(documentId, Path.GetFileName(filePath), folders: folders, filePath: filePath, loader: loader, sourceCodeKind: sourceCodeKind);
        AddDocument(documentInfo);
    
        return documentId;
    }

    public void SetParseOptions(Microsoft.CodeAnalysis.ProjectId projectId, ParseOptions parseOptions)
    {
        OnParseOptionsChanged(projectId, parseOptions);
    }

    public DocumentId GetDocumentId(string filePath)
    {
        var documentIds = CurrentSolution.GetDocumentIdsWithFilePath(filePath);
        return documentIds.FirstOrDefault();
    }

    public IEnumerable<Document> GetDocuments(string filePath)
    {
        return CurrentSolution
            .GetDocumentIdsWithFilePath(filePath)
            .Select(id => CurrentSolution.GetDocument(id))
            .OfType<Document>();
    }

    public IEnumerable<string> GetProjectAssemblyPaths()
    {
        var result = CurrentSolution.Projects.Select(p => p.OutputFilePath).Where(p => p != null).ToList();
        return result;
    }

    public Document GetDocument(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return null;

        var documentId = GetDocumentId(filePath);
        if (documentId == null)
        {
            return null;
        }

        return CurrentSolution.GetDocument(documentId);
    }

    public override bool CanApplyChange(ApplyChangesKind feature)
    {
        return true;
    }

    protected override void ApplyDocumentRemoved(DocumentId documentId)
    {
        var document = this.CurrentSolution.GetDocument(documentId);
        if (document != null)
        {
            DeleteDocumentFile(document.Id, document.FilePath);
            this.OnDocumentRemoved(documentId);
        }
    }

    private void DeleteDocumentFile(DocumentId id, string fullPath)
    {
        try
        {
            File.Delete(fullPath);
        }
        catch (IOException e)
        {
            LogDeletionException(e, fullPath);
        }
        catch (NotSupportedException e)
        {
            LogDeletionException(e, fullPath);
        }
        catch (UnauthorizedAccessException e)
        {
            LogDeletionException(e, fullPath);
        }
    }

    private void LogDeletionException(Exception e, string filePath)
    {
        _logger.LogError(e, $"Error deleting file {filePath}");
    }

    protected override void ApplyDocumentAdded(DocumentInfo info, SourceText text)
    {
        var fullPath = info.FilePath;

        this.OnDocumentAdded(info);

        if (text != null)
        {
            this.SaveDocumentText(info.Id, fullPath, text, text.Encoding ?? Encoding.UTF8);
        }
    }

    private void SaveDocumentText(DocumentId id, string fullPath, SourceText newText, Encoding encoding)
    {
        try
        {
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var writer = new StreamWriter(fullPath, append: false, encoding: encoding))
            {
                newText.Write(writer);
            }
        }
        catch (IOException e)
        {
            _logger.LogError(e, $"Error saving document {fullPath}");
        }
    }

    protected override void ApplyProjectChanges(ProjectChanges projectChanges)
    {
        // since Roslyn currently doesn't handle DefaultNamespace changes via ApplyProjectChanges
        // and OnDefaultNamespaceChanged is internal, we use reflection for now
        if (projectChanges.NewProject.DefaultNamespace != projectChanges.OldProject.DefaultNamespace)
        {
            var onDefaultNamespaceChanged = this.GetType().GetMethod("OnDefaultNamespaceChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            if (onDefaultNamespaceChanged != null)
            {
                onDefaultNamespaceChanged.Invoke(this, new object[] { projectChanges.ProjectId, projectChanges.NewProject.DefaultNamespace });
            }
        }

        base.ApplyProjectChanges(projectChanges);
    }
}