using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TryOmnisharpExtension.IlSpy;

namespace OmniSharp.MSBuild
{
    internal class ProjectSystem
    {
        private readonly IEventEmitter _eventEmitter;
        private readonly ILogger _logger;
        private readonly ProjectManager _manager;
        private readonly DecompileWorkspace _decompileWorkspace;

        public ProjectSystem(
            IEventEmitter eventEmitter,
            ILoggerFactory loggerFactory,
            DecompileWorkspace decompileWorkspace,
            ProjectManager projectManager)
        {
            _decompileWorkspace = decompileWorkspace;
            _manager = projectManager;
            _eventEmitter = eventEmitter;
            _logger = loggerFactory.CreateLogger<ProjectSystem>();
        }

        public async Task Start(LogLevel logLevel, FileInfo solutionFileInfo)
        {
            if (logLevel < LogLevel.Information)
            {
                var buildEnvironmentInfo = MSBuildHelpers.GetBuildEnvironmentInfo();
                _logger.LogDebug($"MSBuild environment: {Environment.NewLine}{buildEnvironmentInfo}");
            }

            var initialProjectPathsAndIds = GetProjectPathsAndIdsFromSolutionOrFilter(solutionFileInfo.FullName);

            _eventEmitter.Emit("SOLUTION_PARSED", new
            {
                NumberOfProjects = initialProjectPathsAndIds.Count(),
                SolutionName = solutionFileInfo.Name
            });

            var projectsLoadTimer = Stopwatch.StartNew();
            foreach (var (projectFilePath, projectIdInfo) in initialProjectPathsAndIds)
            {
                if (!File.Exists(projectFilePath))
                {
                    _logger.LogWarning($"Found project that doesn't exist on disk: {projectFilePath}");
                    continue;
                }

                var loadResult = _manager.ProcessProjectUpdate(projectFilePath, allowAutoRestore: true, projectIdInfo);
                if (loadResult)
                {
                    _eventEmitter.Emit("PROJECT_LOADED", new { ProjectName = projectFilePath });
                }
                else
                {
                    _eventEmitter.Emit("PROJECT_FAILED", new { ProjectName = projectFilePath });
                }
            }
            projectsLoadTimer.Stop();

            var dllLoadTimer = Stopwatch.StartNew();
            var assemblyCount = _decompileWorkspace.LoadDlls();
            dllLoadTimer.Stop();
            var compilationsTimer = Stopwatch.StartNew();
            await _decompileWorkspace.RunProjectCompilations();
            compilationsTimer.Stop();
            _eventEmitter.Emit("ASSEMBLIES_LOADED", new
            {
                AssembliesLoaded = assemblyCount,
                ProjectsLoadTime = projectsLoadTimer.Elapsed,
                DllLoadTime = dllLoadTimer.Elapsed,
                CompilationsTime = compilationsTimer.Elapsed
            });
        }

        private IEnumerable<(string, ProjectIdInfo)> GetProjectPathsAndIdsFromSolutionOrFilter(string solutionOrFilterFilePath)
        {
            _logger.LogInformation($"Detecting projects in '{solutionOrFilterFilePath}'.");

            var solutionFilePath = solutionOrFilterFilePath;

            var projectFilter = ImmutableHashSet<string>.Empty;
            if (SolutionFilterReader.IsSolutionFilterFilename(solutionOrFilterFilePath) &&
                !SolutionFilterReader.TryRead(solutionOrFilterFilePath, out solutionFilePath, out projectFilter))
            {
                throw new InvalidSolutionFileException($"Solution filter file was invalid.");
            }

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var solutionFile = SolutionFile.ParseFile(solutionFilePath);
            var processedProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<(string, ProjectIdInfo)>();

            var solutionConfigurations = new Dictionary<ProjectId, Dictionary<string, string>>();
            foreach (var globalSection in solutionFile.GlobalSections)
            {
                // Try parse project configurations if they are remapped in solution file
                if (globalSection.Name == "ProjectConfigurationPlatforms")
                {
                    _logger.LogDebug($"Parsing ProjectConfigurationPlatforms of '{solutionFilePath}'.");
                    foreach (var entry in globalSection.Properties)
                    {
                        var guid = Guid.Parse(entry.Name.Substring(0, 38));
                        var projId = ProjectId.CreateFromSerialized(guid);
                        var solutionConfig = entry.Name.Substring(39);

                        if (!solutionConfigurations.TryGetValue(projId, out var dict))
                        {
                            dict = new Dictionary<string, string>();
                            solutionConfigurations.Add(projId, dict);
                        }
                        dict.Add(solutionConfig, entry.Value);
                    }
                }
            }

            foreach (var project in solutionFile.Projects)
            {
                if (project.IsNotSupported)
                {
                    continue;
                }

                // Solution files contain relative paths to project files with Windows-style slashes.
                var relativeProjectfilePath = project.RelativePath.Replace('\\', Path.DirectorySeparatorChar);
                var projectFilePath = Path.GetFullPath(Path.Combine(solutionFolder, relativeProjectfilePath));
                if (!projectFilter.IsEmpty &&
                    !projectFilter.Contains(projectFilePath))
                {
                    continue;
                }

                // Have we seen this project? If so, move on.
                if (processedProjects.Contains(projectFilePath))
                {
                    continue;
                }

                if (string.Equals(Path.GetExtension(projectFilePath), ".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    var projectIdInfo = new ProjectIdInfo(ProjectId.CreateFromSerialized(new Guid(project.ProjectGuid)), true);
                    if (solutionConfigurations.TryGetValue(projectIdInfo.Id, out var configurations))
                    {
                        projectIdInfo.SolutionConfiguration = configurations;
                    }
                    result.Add((projectFilePath, projectIdInfo));
                }

                processedProjects.Add(projectFilePath);
            }

            return result;
        }
    }
}