using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using MSB = Microsoft.Build;

internal class ProjectManager : DisposableObject
{
    private class ProjectToUpdate
    {
        public ProjectIdInfo ProjectIdInfo;
        public string FilePath { get; }
        public bool AllowAutoRestore { get; set; }
        public string ChangeTriggerPath { get; }
        public ProjectLoadedEventArgs LoadedEventArgs { get; set; }

        public ProjectToUpdate(string filePath, ProjectIdInfo projectIdInfo)
        {
            ProjectIdInfo = projectIdInfo ?? throw new ArgumentNullException(nameof(projectIdInfo));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }
    }

    private readonly ILogger _logger;
    private readonly IEventEmitter _eventEmitter;
    private readonly MetadataFileReferenceCache _metadataFileReferenceCache;
    private readonly PackageDependencyChecker _packageDependencyChecker;
    private readonly ProjectFileInfoCollection _projectFiles;
    private readonly HashSet<string> _failedToLoadProjectFiles;
    private readonly ProjectLoader _projectLoader;
    private readonly OmniSharpWorkspace2 _workspace;
    private readonly object _workspaceGate = new();
    private readonly CancellationTokenSource _processLoopCancellation;
    private readonly IAnalyzerAssemblyLoader _analyzerAssemblyLoader;
    private readonly DotNetInfo _dotNetInfo;
    private readonly Guid _sessionId = Guid.NewGuid();

    public ProjectManager(
        ILoggerFactory loggerFactory,
        IEventEmitter eventEmitter,
        MetadataFileReferenceCache metadataFileReferenceCache,
        PackageDependencyChecker packageDependencyChecker,
        ProjectLoader projectLoader,
        OmniSharpWorkspace2 workspace,
        IAnalyzerAssemblyLoader analyzerAssemblyLoader,
        DotNetInfo dotNetInfo)
    {
        _logger = loggerFactory.CreateLogger<ProjectManager>();
        _eventEmitter = eventEmitter;
        _metadataFileReferenceCache = metadataFileReferenceCache;
        _packageDependencyChecker = packageDependencyChecker;
        _projectFiles = new ProjectFileInfoCollection();
        _failedToLoadProjectFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _projectLoader = projectLoader;
        _workspace = workspace;
        _dotNetInfo = dotNetInfo;
        _processLoopCancellation = new CancellationTokenSource();
        _analyzerAssemblyLoader = analyzerAssemblyLoader;
    }

    protected override void DisposeCore(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        _processLoopCancellation.Cancel();
        _processLoopCancellation.Dispose();
    }
    
    public bool LoadProject(string projectFilePath, ProjectIdInfo projectId)
    {
        _logger.LogInformation($"Start project load for '{projectFilePath}'");
        var projectToUpdate = new ProjectToUpdate(projectFilePath, projectId);
        if (_workspace.CurrentSolution.Projects.Any(i => i.FilePath == projectFilePath))
        {
            _logger.LogInformation($"Already leaded '{projectFilePath}'");
            return true;
        }

        _logger.LogInformation($"Loading project: {projectToUpdate.FilePath}");

        try
        {
            var buildResult = Build(
                projectToUpdate.FilePath,
                projectToUpdate.ProjectIdInfo);
            
            if (buildResult.success)
            {
                _logger.LogInformation($"Successfully ran buildProject project file '{projectToUpdate.FilePath}'.");
                projectToUpdate.LoadedEventArgs = buildResult.eventArgs;
                AddProjectToWorkspace(buildResult.projectFileInfo);
                AddProjectFilesToWorkspace(buildResult.projectFileInfo);

                _packageDependencyChecker.CheckForUnresolvedDependences(buildResult.projectFileInfo, projectToUpdate.AllowAutoRestore);
                _logger.LogInformation($"Successfully loaded '{projectToUpdate.FilePath}'.");
            }
            else
            {
                _failedToLoadProjectFiles.Add(projectToUpdate.FilePath);
                _logger.LogWarning($"Failed to load project file '{projectToUpdate.FilePath}'.");
                return false;
            }

            _eventEmitter.MSBuildProjectDiagnostics(projectToUpdate.FilePath, buildResult.diagnostics);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load project file '{projectToUpdate.FilePath}'.");
            _eventEmitter.Error(ex, fileName: projectToUpdate.FilePath);
            return false;
        }
    }

    private void AddProjectToWorkspace(ProjectFileInfo projectFileInfo)
    {
        _logger.LogInformation($"Adding project '{projectFileInfo.FilePath}'");
        
        _projectFiles.Add(projectFileInfo);

        var projectInfo = projectFileInfo.CreateProjectInfo(_analyzerAssemblyLoader);

        lock (_workspaceGate)
        {
            var newSolution = _workspace.CurrentSolution.AddProject(projectInfo);

            // SubscribeToAnalyzerReferenceLoadFailures(projectInfo.AnalyzerReferences.Cast<AnalyzerFileReference>(), _logger);

            if (!_workspace.TryApplyChanges(newSolution))
            {
                _logger.LogError($"Failed to add project to workspace: '{projectFileInfo.FilePath}'");
            }
        }
    }
    
    private void AddProjectFilesToWorkspace(ProjectFileInfo projectFileInfo)
    {
        var project = _workspace.CurrentSolution.GetProject(projectFileInfo.Id);
        if (project == null)
        {
            _logger.LogError($"Could not locate project in workspace: {projectFileInfo.FilePath}");
            return;
        }

        // for other update triggers, perform a full check of all options
        UpdateSourceFiles(project, projectFileInfo.SourceFiles);
        UpdateParseOptions(project, projectFileInfo.LanguageVersion, projectFileInfo.PreprocessorSymbolNames, !string.IsNullOrWhiteSpace(projectFileInfo.DocumentationFile));
        UpdateProjectReferences(project, projectFileInfo.ProjectReferences);
        UpdateReferences(project, projectFileInfo.ProjectReferences, projectFileInfo.References);
        UpdateProjectProperties(project, projectFileInfo);
        UpdateCompilationOptions(project, projectFileInfo);
    }

    private void UpdateCompilationOptions(Project project, ProjectFileInfo projectFileInfo)
    {
        // if project already has compilation options, then we shall use that to compute new compilation options based on the project file
        // and then only set those if it's really necessary
        if (project.CompilationOptions != null && project.CompilationOptions is CSharpCompilationOptions existingCompilationOptions)
        {
            var newCompilationOptions = projectFileInfo.CreateCompilationOptions(existingCompilationOptions);
            if (newCompilationOptions != existingCompilationOptions)
            {
                _workspace.UpdateCompilationOptionsForProject(project.Id, newCompilationOptions);
                _logger.LogDebug($"Updated project compilation options on project {project.Name}.");
            }
        }
    }

    private void UpdateProjectProperties(Project project, ProjectFileInfo projectFileInfo)
    {
        if (projectFileInfo.DefaultNamespace != project.DefaultNamespace)
        {
            var newSolution = _workspace.CurrentSolution.WithProjectDefaultNamespace(project.Id, projectFileInfo.DefaultNamespace);
            if (_workspace.TryApplyChanges(newSolution))
            {
                _logger.LogDebug($"Updated default namespace from {project.DefaultNamespace} to {projectFileInfo.DefaultNamespace} on {project.FilePath} project.");
            }
            else
            {
                _logger.LogWarning($"Couldn't update default namespace from {project.DefaultNamespace} to {projectFileInfo.DefaultNamespace} on {project.FilePath} project.");
            }
        }
    }
    
    private void UpdateSourceFiles(Project project, IList<string> sourceFiles)
    {
        // Remove transient documents from list of current documents, to assure proper new documents are added.
        // Transient documents will be removed on workspace DocumentAdded event.
        // var currentDocuments = project.Documents
        //     .Where(document => !_workspace.BufferManager.IsTransientDocument(document.Id))
        //     .ToDictionary(d => d.FilePath, d => d.Id);
    
        // Add source files to the project.
        foreach (var sourceFile in sourceFiles)
        {
            if (!File.Exists(sourceFile))
            {
                continue;
            }
    
            _workspace.AddDocument(project, sourceFile);
        }
    }

    private void UpdateParseOptions(Project project, LanguageVersion languageVersion, IEnumerable<string> preprocessorSymbolNames, bool generateXmlDocumentation)
    {
        var existingParseOptions = (CSharpParseOptions)project.ParseOptions;

        if (existingParseOptions.LanguageVersion == languageVersion &&
            Enumerable.SequenceEqual(existingParseOptions.PreprocessorSymbolNames, preprocessorSymbolNames) &&
            (existingParseOptions.DocumentationMode == DocumentationMode.Diagnose) == generateXmlDocumentation)
        {
            // No changes to make. Moving on.
            return;
        }

        var parseOptions = new CSharpParseOptions(languageVersion);

        if (preprocessorSymbolNames.Any())
        {
            parseOptions = parseOptions.WithPreprocessorSymbols(preprocessorSymbolNames);
        }

        if (generateXmlDocumentation)
        {
            parseOptions = parseOptions.WithDocumentationMode(DocumentationMode.Diagnose);
        }

        _workspace.SetParseOptions(project.Id, parseOptions);
    }

    private void UpdateProjectReferences(Project project, ImmutableArray<string> projectReferencePaths)
    {
        _logger.LogInformation($"Update project: {project.Name}");

        var existingProjectReferences = new HashSet<ProjectReference>(project.ProjectReferences);
        var addedProjectReferences = new HashSet<ProjectReference>();

        foreach (var projectReferencePath in projectReferencePaths)
        {
            if (_failedToLoadProjectFiles.Contains(projectReferencePath))
            {
                _logger.LogWarning($"Ignoring previously failed to load project '{projectReferencePath}' referenced by '{project.Name}'.");
                continue;
            }

            if (!_projectFiles.TryGetValue(projectReferencePath, out var referencedProject))
            {
                if (File.Exists(projectReferencePath) &&
                    projectReferencePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Found referenced project outside root directory: {projectReferencePath}");

                    // We've found a project reference that we didn't know about already, but it exists on disk.
                    // This is likely a project that is outside of OmniSharp's TargetDirectory.
                    referencedProject = ProjectFileInfo.CreateNoBuild(projectReferencePath, _projectLoader, _dotNetInfo);
                    LoadProject(referencedProject.FilePath, referencedProject.ProjectIdInfo);
                    // AddProjectToWorkspace(referencedProject);
                    // AddProjectFilesToWorkspace(referencedProject);
                }
            }

            if (referencedProject == null)
            {
                _logger.LogWarning($"Unable to resolve project reference '{projectReferencePath}' for '{project.Name}'.");
                continue;
            }

            ImmutableArray<string> aliases = default;
            if (_projectFiles.TryGetValue(project.FilePath, out var projectFileInfo))
            {
                if (projectFileInfo.ProjectReferenceAliases.TryGetValue(projectReferencePath, out var projectReferenceAliases))
                {
                    if (!string.IsNullOrEmpty(projectReferenceAliases))
                    {
                        aliases = ImmutableArray.CreateRange(projectReferenceAliases.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()));
                        _logger.LogDebug($"Setting aliases: {projectReferencePath}, {projectReferenceAliases} ");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Failed to get project info: {project.FilePath}");
            }

            var projectReference = new ProjectReference(referencedProject.Id, aliases);

            if (existingProjectReferences.Remove(projectReference))
            {
                // This reference already exists
                continue;
            }

            if (!addedProjectReferences.Contains(projectReference))
            {
                _workspace.AddProjectReference(project.Id, projectReference);
                addedProjectReferences.Add(projectReference);
            }
        }

        foreach (var existingProjectReference in existingProjectReferences)
        {
            _workspace.RemoveProjectReference(project.Id, existingProjectReference);
        }
    }

    private void UpdateReferences(Project project, ImmutableArray<string> projectReferencePaths, ImmutableArray<string> referencePaths)
    {
        var referencesToRemove = new HashSet<MetadataReference>(project.MetadataReferences, MetadataReferenceEqualityComparer.Instance);
        var referencesToAdd = new HashSet<MetadataReference>(MetadataReferenceEqualityComparer.Instance);

        foreach (var referencePath in referencePaths)
        {
            if (!File.Exists(referencePath))
            {
                _logger.LogWarning($"Unable to resolve assembly '{referencePath}'");
                continue;
            }

            // There is no need to explicitly add assembly to workspace when the assembly is produced by a project reference.
            // Doing so actually can cause /codecheck request to return errors like below for types in the referenced project if it is for example signed:
            // The type 'TestClass' exists in both 'SignedLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' and 'ClassLibrary1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a5d85a5baa39a6a6'
            if (TryFindLoadedProjectReferenceWithTargetPath(referencePath, projectReferencePaths, project.Name, out ProjectFileInfo projectReferenceWithTarget))
            {
                _logger.LogDebug($"Skipped reference {referencePath} of project {project.Name} because it is already represented as a target " +
                                 $"of loaded project reference {projectReferenceWithTarget.Name}");
                continue;
            }

            var reference = _metadataFileReferenceCache.GetMetadataReference(referencePath);

            if (referencesToRemove.Remove(reference))
            {
                continue;
            }

            if (!referencesToAdd.Contains(reference))
            {
                if (_projectFiles.TryGetValue(project.FilePath, out var projectFileInfo))
                {
                    if (projectFileInfo.ReferenceAliases.TryGetValue(referencePath, out var aliases))
                    {
                        if (!string.IsNullOrEmpty(aliases))
                        {
                            reference = reference.WithAliases(aliases.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()));
                            _logger.LogDebug($"Setting aliases: {referencePath}, {aliases} ");
                        }
                    }
                }
                else
                {
                    _logger.LogDebug($"Failed to get project info: {project.FilePath}");
                }
                _logger.LogDebug($"Adding reference '{referencePath}' to '{project.Name}'.");
                _workspace.AddMetadataReference(project.Id, reference);
                referencesToAdd.Add(reference);
            }
        }

        foreach (var reference in referencesToRemove)
        {
            _workspace.RemoveMetadataReference(project.Id, reference);
        }
    }

    /// <summary> Attempts to locate a referenced project with particular target path i.e. the path of the assembly that the referenced project produces. /// </summary>
    /// <param name="targetPath">Target path to look for.</param>
    /// <param name="projectReferencePaths">List of projects referenced by <see cref="projectName"/></param>
    /// <param name="projectName">Name of the project for which the search is initiated</param>
    /// <param name="projectReferenceWithTarget">If found, contains project reference with the <see cref="targetPath"/>; null otherwise</param>
    /// <returns></returns>
    private bool TryFindLoadedProjectReferenceWithTargetPath(string targetPath, ImmutableArray<string> projectReferencePaths, string projectName, out ProjectFileInfo projectReferenceWithTarget)
    {
        projectReferenceWithTarget = null;
        foreach (string projectReferencePath in projectReferencePaths)
        {
            if (!_projectFiles.TryGetValue(projectReferencePath, out ProjectFileInfo referencedProject))
            {
                _logger.LogWarning($"Expected project reference {projectReferencePath} to be already loaded for project {projectName}");
                continue;
            }

            if (referencedProject.TargetPath != null && referencedProject.TargetPath.Equals(targetPath, StringComparison.OrdinalIgnoreCase))
            {
                projectReferenceWithTarget = referencedProject;
                return true;
            }
        }

        return false;
    }
    
    public (bool success, ProjectFileInfo projectFileInfo, ImmutableArray<MSBuildDiagnostic> diagnostics, ProjectLoadedEventArgs eventArgs)
        Build(string filePath, ProjectIdInfo projectIdInfo)
    {
        if (!File.Exists(filePath))
        {
            return (false, null, ImmutableArray<MSBuildDiagnostic>.Empty, null);
        }

        var (buildResult, projectInstance, project, diagnostics) = _projectLoader.BuildProject(filePath, projectIdInfo?.SolutionConfiguration);
        
        if (projectInstance == null)
        {
            return (buildResult, null, diagnostics, null);
        }

        var data = ProjectData.Create(filePath, projectInstance, project);
        var projectFileInfo = new ProjectFileInfo(projectIdInfo, filePath, data, _sessionId, _dotNetInfo);
        var eventArgs = new ProjectLoadedEventArgs(projectIdInfo.Id,
            project,
            _sessionId,
            projectInstance,
            diagnostics,
            isReload: false,
            projectIdInfo.IsDefinedInSolution,
            projectFileInfo.SourceFiles,
            _dotNetInfo.Version,
            data.References);

        return (buildResult, projectFileInfo, diagnostics, eventArgs);
    }
}