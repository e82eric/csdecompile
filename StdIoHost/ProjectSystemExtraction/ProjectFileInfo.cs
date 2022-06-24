using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Build.Globbing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NuGet.Packaging.Core;
using MSB = Microsoft.Build;

internal partial class ProjectFileInfo
{
    private readonly ProjectData _data;

    public string FilePath { get; }
    public string Directory { get; }

    public Microsoft.CodeAnalysis.ProjectId Id { get => ProjectIdInfo.Id; }

    public Guid Guid => _data.Guid;
    public string Name => _data.Name;

    public string AssemblyName => _data.AssemblyName;
    public string TargetPath => _data.TargetPath;
    public string OutputPath => _data.OutputPath;
    public string IntermediateOutputPath => _data.IntermediateOutputPath;
    public string ProjectAssetsFile => _data.ProjectAssetsFile;

    public string Configuration => _data.Configuration;
    public string Platform => _data.Platform;
    public string PlatformTarget => _data.PlatformTarget;
    public FrameworkName TargetFramework => _data.TargetFramework;
    public ImmutableArray<string> TargetFrameworks => _data.TargetFrameworks;

    public OutputKind OutputKind => _data.OutputKind;
    public LanguageVersion LanguageVersion => _data.LanguageVersion;
    public NullableContextOptions NullableContextOptions => _data.NullableContextOptions;
    public bool AllowUnsafeCode => _data.AllowUnsafeCode;
    public bool CheckForOverflowUnderflow => _data.CheckForOverflowUnderflow;
    public string DocumentationFile => _data.DocumentationFile;
    public ImmutableArray<string> PreprocessorSymbolNames => _data.PreprocessorSymbolNames;
    public ImmutableArray<string> SuppressedDiagnosticIds => _data.SuppressedDiagnosticIds;
    public ImmutableArray<string> WarningsAsErrors => _data.WarningsAsErrors;
    public ImmutableArray<string> WarningsNotAsErrors => _data.WarningsNotAsErrors;

    public bool SignAssembly => _data.SignAssembly;
    public string AssemblyOriginatorKeyFile => _data.AssemblyOriginatorKeyFile;
    public RuleSet RuleSet => _data.RuleSet;

    public ImmutableArray<string> SourceFiles => _data.SourceFiles;
    public ImmutableArray<string> References => _data.References;
    public ImmutableArray<string> ProjectReferences => _data.ProjectReferences;
    public ImmutableArray<PackageReference> PackageReferences => _data.PackageReferences;
    public ImmutableArray<string> Analyzers => _data.Analyzers;
    public ImmutableArray<string> AdditionalFiles => _data.AdditionalFiles;
    public ImmutableArray<string> AnalyzerConfigFiles => _data.AnalyzerConfigFiles;
    public ImmutableDictionary<string, string> ReferenceAliases => _data.ReferenceAliases;
    public ImmutableDictionary<string, string> ProjectReferenceAliases => _data.ProjectReferenceAliases;
    public bool TreatWarningsAsErrors => _data.TreatWarningsAsErrors;
    public bool RunAnalyzers => _data.RunAnalyzers;
    public bool RunAnalyzersDuringLiveAnalysis => _data.RunAnalyzersDuringLiveAnalysis;
    public string DefaultNamespace => _data.DefaultNamespace;
    public ImmutableArray<IMSBuildGlob> FileInclusionGlobPatterns => _data.FileInclusionGlobs;

    public ProjectIdInfo ProjectIdInfo { get; }
    public DotNetInfo DotNetInfo { get; }
    public Guid SessionId { get; }

    private ProjectFileInfo(
        ProjectIdInfo projectIdInfo,
        string filePath,
        ProjectData data,
        Guid sessionId,
        DotNetInfo dotNetInfo)
    {
        this.ProjectIdInfo = projectIdInfo;
        this.FilePath = filePath;
        this.Directory = Path.GetDirectoryName(filePath);
        this.SessionId = sessionId;
        this.DotNetInfo = dotNetInfo;

        _data = data;
    }

    internal static ProjectFileInfo CreateEmpty(string filePath)
    {
        var id = ProjectId.CreateNewId(debugName: filePath);

        return new ProjectFileInfo(new ProjectIdInfo(id, isDefinedInSolution: false), filePath, data: null, sessionId: Guid.NewGuid(), DotNetInfo.Empty);
    }

    internal static ProjectFileInfo CreateNoBuild(string filePath, ProjectLoader loader, DotNetInfo dotNetInfo)
    {
        var id = ProjectId.CreateNewId(debugName: filePath);
        var project = loader.EvaluateProjectFile(filePath);

        var data = ProjectData.Create(project);
        //we are not reading the solution here
        var projectIdInfo = new ProjectIdInfo(id, isDefinedInSolution: false);

        return new ProjectFileInfo(projectIdInfo, filePath, data, sessionId: Guid.NewGuid(), dotNetInfo);
    }

    public static (ProjectFileInfo, ImmutableArray<MSBuildDiagnostic>, ProjectLoadedEventArgs) Load(string filePath, ProjectIdInfo projectIdInfo, ProjectLoader loader, Guid sessionId, DotNetInfo dotNetInfo)
    {
        if (!File.Exists(filePath))
        {
            return (null, ImmutableArray<MSBuildDiagnostic>.Empty, null);
        }

        var (projectInstance, project, diagnostics) = loader.BuildProject(filePath, projectIdInfo?.SolutionConfiguration);
        if (projectInstance == null)
        {
            return (null, diagnostics, null);
        }

        var data = ProjectData.Create(filePath, projectInstance, project);
        var projectFileInfo = new ProjectFileInfo(projectIdInfo, filePath, data, sessionId, dotNetInfo);
        var eventArgs = new ProjectLoadedEventArgs(projectIdInfo.Id,
            project,
            sessionId,
            projectInstance,
            diagnostics,
            isReload: false,
            projectIdInfo.IsDefinedInSolution,
            projectFileInfo.SourceFiles,
            dotNetInfo.Version,
            data.References);

        return (projectFileInfo, diagnostics, eventArgs);
    }

    public (ProjectFileInfo, ImmutableArray<MSBuildDiagnostic>, ProjectLoadedEventArgs) Reload(ProjectLoader loader)
    {
        var (projectInstance, project, diagnostics) = loader.BuildProject(FilePath, ProjectIdInfo?.SolutionConfiguration);
        if (projectInstance == null)
        {
            return (null, diagnostics, null);
        }

        var data = ProjectData.Create(FilePath, projectInstance, project);
        var projectFileInfo = new ProjectFileInfo(ProjectIdInfo, FilePath, data, SessionId, DotNetInfo);
        var eventArgs = new ProjectLoadedEventArgs(Id,
            project,
            SessionId,
            projectInstance,
            diagnostics,
            isReload: true,
            ProjectIdInfo.IsDefinedInSolution,
            data.SourceFiles,
            DotNetInfo.Version,
            data.References);

        return (projectFileInfo, diagnostics, eventArgs);
    }

    public bool IsUnityProject()
        => References.Any(filePath =>
        {
            var fileName = Path.GetFileName(filePath);

            return string.Equals(fileName, "UnityEngine.dll", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(fileName, "UnityEditor.dll", StringComparison.OrdinalIgnoreCase);
        });


    public bool IsFileIncluded(string filePath)
    {
        if (SourceFiles.Contains(filePath, StringComparer.OrdinalIgnoreCase))
            return true;

        var relativePath = FileSystemHelper.GetRelativePath(filePath, FilePath);
        return FileInclusionGlobPatterns.Any(glob => glob.IsMatch(relativePath));
    }
}
internal partial class ProjectFileInfo
{
    private class ProjectData
    {
        public Guid Guid { get; }
        public string Name { get; }

        public string AssemblyName { get; }
        public string TargetPath { get; }
        public string OutputPath { get; }
        public string IntermediateOutputPath { get; }
        public string ProjectAssetsFile { get; }

        public string Configuration { get; }
        public string Platform { get; }
        public string PlatformTarget { get; }
        public FrameworkName TargetFramework { get; }
        public ImmutableArray<string> TargetFrameworks { get; }

        public OutputKind OutputKind { get; }
        public LanguageVersion LanguageVersion { get; }
        public NullableContextOptions NullableContextOptions { get; }
        public bool AllowUnsafeCode { get; }
        public bool CheckForOverflowUnderflow { get; }
        public string DocumentationFile { get; }
        public ImmutableArray<string> PreprocessorSymbolNames { get; }
        public ImmutableArray<string> SuppressedDiagnosticIds { get; }

        public bool SignAssembly { get; }
        public string AssemblyOriginatorKeyFile { get; }

        public ImmutableArray<IMSBuildGlob> FileInclusionGlobs { get; }
        public ImmutableArray<string> SourceFiles { get; }
        public ImmutableArray<string> ProjectReferences { get; }
        public ImmutableArray<string> References { get; }
        public ImmutableArray<PackageReference> PackageReferences { get; }
        public ImmutableArray<string> Analyzers { get; }
        public ImmutableArray<string> AdditionalFiles { get; }
        public ImmutableArray<string> AnalyzerConfigFiles { get; }
        public ImmutableArray<string> WarningsAsErrors { get; }
        public ImmutableArray<string> WarningsNotAsErrors { get; }
        public RuleSet RuleSet { get; }
        public ImmutableDictionary<string, string> ReferenceAliases { get; }
        public ImmutableDictionary<string, string> ProjectReferenceAliases { get; }
        public bool TreatWarningsAsErrors { get; }
        public bool RunAnalyzers { get; }
        public bool RunAnalyzersDuringLiveAnalysis { get; }
        public string DefaultNamespace { get; }

        private ProjectData()
        {
            // Be sure to initialize all collection properties with ImmutableArray<T>.Empty.
            // Otherwise, Json.net won't be able to serialize the values.
            TargetFrameworks = ImmutableArray<string>.Empty;
            PreprocessorSymbolNames = ImmutableArray<string>.Empty;
            SuppressedDiagnosticIds = ImmutableArray<string>.Empty;
            SourceFiles = ImmutableArray<string>.Empty;
            ProjectReferences = ImmutableArray<string>.Empty;
            References = ImmutableArray<string>.Empty;
            PackageReferences = ImmutableArray<PackageReference>.Empty;
            Analyzers = ImmutableArray<string>.Empty;
            AdditionalFiles = ImmutableArray<string>.Empty;
            AnalyzerConfigFiles = ImmutableArray<string>.Empty;
            ReferenceAliases = ImmutableDictionary<string, string>.Empty;
            ProjectReferenceAliases = ImmutableDictionary<string, string>.Empty;
            WarningsAsErrors = ImmutableArray<string>.Empty;
            FileInclusionGlobs = ImmutableArray<IMSBuildGlob>.Empty;
            WarningsNotAsErrors = ImmutableArray<string>.Empty;
        }

        private ProjectData(
            Guid guid, string name,
            string assemblyName, string targetPath, string outputPath, string intermediateOutputPath,
            string projectAssetsFile,
            string configuration, string platform, string platformTarget,
            FrameworkName targetFramework,
            ImmutableArray<string> targetFrameworks,
            OutputKind outputKind,
            LanguageVersion languageVersion,
            NullableContextOptions nullableContextOptions,
            bool allowUnsafeCode,
            bool checkForOverflowUnderflow,
            string documentationFile,
            ImmutableArray<string> preprocessorSymbolNames,
            ImmutableArray<string> suppressedDiagnosticIds,
            ImmutableArray<string> warningsAsErrors,
            ImmutableArray<string> warningsNotAsErrors,
            bool signAssembly,
            string assemblyOriginatorKeyFile,
            bool treatWarningsAsErrors,
            string defaultNamespace,
            bool runAnalyzers,
            bool runAnalyzersDuringLiveAnalysis,
            RuleSet ruleset)
            : this()
        {
            Guid = guid;
            Name = name;

            AssemblyName = assemblyName;
            TargetPath = targetPath;
            OutputPath = outputPath;
            IntermediateOutputPath = intermediateOutputPath;
            ProjectAssetsFile = projectAssetsFile;

            Configuration = configuration;
            Platform = platform;
            PlatformTarget = platformTarget;
            TargetFramework = targetFramework;
            TargetFrameworks = targetFrameworks.EmptyIfDefault();

            OutputKind = outputKind;
            LanguageVersion = languageVersion;
            NullableContextOptions = nullableContextOptions;
            AllowUnsafeCode = allowUnsafeCode;
            CheckForOverflowUnderflow = checkForOverflowUnderflow;
            DocumentationFile = documentationFile;
            PreprocessorSymbolNames = preprocessorSymbolNames.EmptyIfDefault();
            SuppressedDiagnosticIds = suppressedDiagnosticIds.EmptyIfDefault();
            WarningsAsErrors = warningsAsErrors.EmptyIfDefault();
            WarningsNotAsErrors = warningsNotAsErrors.EmptyIfDefault();

            SignAssembly = signAssembly;
            AssemblyOriginatorKeyFile = assemblyOriginatorKeyFile;
            TreatWarningsAsErrors = treatWarningsAsErrors;
            RuleSet = ruleset;
            DefaultNamespace = defaultNamespace;

            RunAnalyzers = runAnalyzers;
            RunAnalyzersDuringLiveAnalysis = runAnalyzersDuringLiveAnalysis;
        }

        private ProjectData(
            Guid guid, string name,
            string assemblyName, string targetPath, string outputPath, string intermediateOutputPath,
            string projectAssetsFile,
            string configuration, string platform, string platformTarget,
            FrameworkName targetFramework,
            ImmutableArray<string> targetFrameworks,
            OutputKind outputKind,
            LanguageVersion languageVersion,
            NullableContextOptions nullableContextOptions,
            bool allowUnsafeCode,
            bool checkForOverflowUnderflow,
            string documentationFile,
            ImmutableArray<string> preprocessorSymbolNames,
            ImmutableArray<string> suppressedDiagnosticIds,
            ImmutableArray<string> warningsAsErrors,
            ImmutableArray<string> warningsNotAsErrors,
            bool signAssembly,
            string assemblyOriginatorKeyFile,
            ImmutableArray<string> sourceFiles,
            ImmutableArray<string> projectReferences,
            ImmutableArray<string> references,
            ImmutableArray<PackageReference> packageReferences,
            ImmutableArray<string> analyzers,
            ImmutableArray<string> additionalFiles,
            ImmutableArray<string> analyzerConfigFiles,
            bool treatWarningsAsErrors,
            string defaultNamespace,
            bool runAnalyzers,
            bool runAnalyzersDuringLiveAnalysis,
            RuleSet ruleset,
            ImmutableDictionary<string, string> referenceAliases,
            ImmutableDictionary<string, string> projectReferenceAliases,
            ImmutableArray<IMSBuildGlob> fileInclusionGlobs)
            : this(guid, name, assemblyName, targetPath, outputPath, intermediateOutputPath, projectAssetsFile,
                  configuration, platform, platformTarget, targetFramework, targetFrameworks, outputKind, languageVersion, nullableContextOptions, allowUnsafeCode, checkForOverflowUnderflow,
                  documentationFile, preprocessorSymbolNames, suppressedDiagnosticIds, warningsAsErrors, warningsNotAsErrors, signAssembly, assemblyOriginatorKeyFile, treatWarningsAsErrors, defaultNamespace, runAnalyzers, runAnalyzersDuringLiveAnalysis, ruleset)
        {
            SourceFiles = sourceFiles.EmptyIfDefault();
            ProjectReferences = projectReferences.EmptyIfDefault();
            References = references.EmptyIfDefault();
            PackageReferences = packageReferences.EmptyIfDefault();
            Analyzers = analyzers.EmptyIfDefault();
            AdditionalFiles = additionalFiles.EmptyIfDefault();
            AnalyzerConfigFiles = analyzerConfigFiles.EmptyIfDefault();
            ReferenceAliases = referenceAliases;
            ProjectReferenceAliases = projectReferenceAliases;
            FileInclusionGlobs = fileInclusionGlobs;
        }

        public static ProjectData Create(MSB.Evaluation.Project project)
        {
            var guid = PropertyConverter.ToGuid(project.GetPropertyValue(PropertyNames.ProjectGuid));
            var name = project.GetPropertyValue(PropertyNames.ProjectName);
            var assemblyName = project.GetPropertyValue(PropertyNames.AssemblyName);
            var targetPath = project.GetPropertyValue(PropertyNames.TargetPath);
            var outputPath = project.GetPropertyValue(PropertyNames.OutputPath);
            var intermediateOutputPath = project.GetPropertyValue(PropertyNames.IntermediateOutputPath);
            var projectAssetsFile = project.GetPropertyValue(PropertyNames.ProjectAssetsFile);
            var configuration = project.GetPropertyValue(PropertyNames.Configuration);
            var platform = project.GetPropertyValue(PropertyNames.Platform);
            var platformTarget = project.GetPropertyValue(PropertyNames.PlatformTarget);
            var defaultNamespace = project.GetPropertyValue(PropertyNames.RootNamespace);

            var targetFramework = new FrameworkName(project.GetPropertyValue(PropertyNames.TargetFrameworkMoniker));

            var targetFrameworkValue = project.GetPropertyValue(PropertyNames.TargetFramework);
            var targetFrameworks = PropertyConverter.SplitList(project.GetPropertyValue(PropertyNames.TargetFrameworks), ';');

            if (!string.IsNullOrWhiteSpace(targetFrameworkValue) && targetFrameworks.Length == 0)
            {
                targetFrameworks = ImmutableArray.Create(targetFrameworkValue);
            }

            var languageVersion = PropertyConverter.ToLanguageVersion(project.GetPropertyValue(PropertyNames.LangVersion));
            var allowUnsafeCode = PropertyConverter.ToBoolean(project.GetPropertyValue(PropertyNames.AllowUnsafeBlocks), defaultValue: false);
            var checkForOverflowUnderflow = PropertyConverter.ToBoolean(project.GetPropertyValue(PropertyNames.CheckForOverflowUnderflow), defaultValue: false);
            var outputKind = PropertyConverter.ToOutputKind(project.GetPropertyValue(PropertyNames.OutputType));
            var nullableContextOptions = PropertyConverter.ToNullableContextOptions(project.GetPropertyValue(PropertyNames.Nullable));
            var documentationFile = project.GetPropertyValue(PropertyNames.DocumentationFile);
            var preprocessorSymbolNames = PropertyConverter.ToPreprocessorSymbolNames(project.GetPropertyValue(PropertyNames.DefineConstants));
            var suppressedDiagnosticIds = PropertyConverter.ToSuppressedDiagnosticIds(project.GetPropertyValue(PropertyNames.NoWarn));
            var warningsAsErrors = PropertyConverter.SplitList(project.GetPropertyValue(PropertyNames.WarningsAsErrors), ',');
            var warningsNotAsErrors = PropertyConverter.SplitList(project.GetPropertyValue(PropertyNames.WarningsNotAsErrors), ',');
            var signAssembly = PropertyConverter.ToBoolean(project.GetPropertyValue(PropertyNames.SignAssembly), defaultValue: false);
            var assemblyOriginatorKeyFile = project.GetPropertyValue(PropertyNames.AssemblyOriginatorKeyFile);
            var treatWarningsAsErrors = PropertyConverter.ToBoolean(project.GetPropertyValue(PropertyNames.TreatWarningsAsErrors), defaultValue: false);
            var runAnalyzers = PropertyConverter.ToBoolean(project.GetPropertyValue(PropertyNames.RunAnalyzers), defaultValue: true);
            var runAnalyzersDuringLiveAnalysis = PropertyConverter.ToBoolean(project.GetPropertyValue(PropertyNames.RunAnalyzersDuringLiveAnalysis), defaultValue: true);

            return new ProjectData(
                guid, name, assemblyName, targetPath, outputPath, intermediateOutputPath, projectAssetsFile,
                configuration, platform, platformTarget, targetFramework, targetFrameworks, outputKind, languageVersion, nullableContextOptions, allowUnsafeCode, checkForOverflowUnderflow,
                documentationFile, preprocessorSymbolNames, suppressedDiagnosticIds, warningsAsErrors, warningsNotAsErrors, signAssembly, assemblyOriginatorKeyFile, treatWarningsAsErrors, defaultNamespace, runAnalyzers, runAnalyzersDuringLiveAnalysis, ruleset: null);
        }

        public static ProjectData Create(string projectFilePath, MSB.Execution.ProjectInstance projectInstance, MSB.Evaluation.Project project)
        {
            var projectFolderPath = Path.GetDirectoryName(projectFilePath);

            var guid = PropertyConverter.ToGuid(projectInstance.GetPropertyValue(PropertyNames.ProjectGuid));
            var name = projectInstance.GetPropertyValue(PropertyNames.ProjectName);
            var assemblyName = projectInstance.GetPropertyValue(PropertyNames.AssemblyName);
            var targetPath = projectInstance.GetPropertyValue(PropertyNames.TargetPath);
            var outputPath = projectInstance.GetPropertyValue(PropertyNames.OutputPath);
            var intermediateOutputPath = projectInstance.GetPropertyValue(PropertyNames.IntermediateOutputPath);
            var projectAssetsFile = projectInstance.GetPropertyValue(PropertyNames.ProjectAssetsFile);
            var configuration = projectInstance.GetPropertyValue(PropertyNames.Configuration);
            var platform = projectInstance.GetPropertyValue(PropertyNames.Platform);
            var platformTarget = projectInstance.GetPropertyValue(PropertyNames.PlatformTarget);
            var defaultNamespace = projectInstance.GetPropertyValue(PropertyNames.RootNamespace);

            var targetFramework = new FrameworkName(projectInstance.GetPropertyValue(PropertyNames.TargetFrameworkMoniker));

            var targetFrameworkValue = projectInstance.GetPropertyValue(PropertyNames.TargetFramework);
            var targetFrameworks = PropertyConverter.SplitList(projectInstance.GetPropertyValue(PropertyNames.TargetFrameworks), ';');

            if (!string.IsNullOrWhiteSpace(targetFrameworkValue) && targetFrameworks.Length == 0)
            {
                targetFrameworks = ImmutableArray.Create(targetFrameworkValue);
            }

            var languageVersion = PropertyConverter.ToLanguageVersion(projectInstance.GetPropertyValue(PropertyNames.LangVersion));
            var allowUnsafeCode = PropertyConverter.ToBoolean(projectInstance.GetPropertyValue(PropertyNames.AllowUnsafeBlocks), defaultValue: false);
            var checkForOverflowUnderflow = PropertyConverter.ToBoolean(projectInstance.GetPropertyValue(PropertyNames.CheckForOverflowUnderflow), defaultValue: false);
            var outputKind = PropertyConverter.ToOutputKind(projectInstance.GetPropertyValue(PropertyNames.OutputType));
            var nullableContextOptions = PropertyConverter.ToNullableContextOptions(projectInstance.GetPropertyValue(PropertyNames.Nullable));
            var documentationFile = projectInstance.GetPropertyValue(PropertyNames.DocumentationFile);
            var preprocessorSymbolNames = PropertyConverter.ToPreprocessorSymbolNames(projectInstance.GetPropertyValue(PropertyNames.DefineConstants));
            var suppressedDiagnosticIds = PropertyConverter.ToSuppressedDiagnosticIds(projectInstance.GetPropertyValue(PropertyNames.NoWarn));
            var warningsAsErrors = PropertyConverter.SplitList(projectInstance.GetPropertyValue(PropertyNames.WarningsAsErrors), ',');
            var warningsNotAsErrors = PropertyConverter.SplitList(projectInstance.GetPropertyValue(PropertyNames.WarningsNotAsErrors), ',');
            var signAssembly = PropertyConverter.ToBoolean(projectInstance.GetPropertyValue(PropertyNames.SignAssembly), defaultValue: false);
            var treatWarningsAsErrors = PropertyConverter.ToBoolean(projectInstance.GetPropertyValue(PropertyNames.TreatWarningsAsErrors), defaultValue: false);
            var runAnalyzers = PropertyConverter.ToBoolean(projectInstance.GetPropertyValue(PropertyNames.RunAnalyzers), defaultValue: true);
            var runAnalyzersDuringLiveAnalysis = PropertyConverter.ToBoolean(projectInstance.GetPropertyValue(PropertyNames.RunAnalyzersDuringLiveAnalysis), defaultValue: true);
            var assemblyOriginatorKeyFile = projectInstance.GetPropertyValue(PropertyNames.AssemblyOriginatorKeyFile);

            var ruleset = ResolveRulesetIfAny(projectInstance);

            var sourceFiles = GetFullPaths(
                projectInstance.GetItems(ItemNames.Compile), filter: FileNameIsNotGenerated);

            var projectReferences = ImmutableArray.CreateBuilder<string>();
            var projectReferenceAliases = ImmutableDictionary.CreateBuilder<string, string>();

            var references = ImmutableArray.CreateBuilder<string>();
            var referenceAliases = ImmutableDictionary.CreateBuilder<string, string>();
            foreach (var referencePathItem in projectInstance.GetItems(ItemNames.ReferencePath))
            {
                var referenceSourceTarget = referencePathItem.GetMetadataValue(MetadataNames.ReferenceSourceTarget);
                var aliases = referencePathItem.GetMetadataValue(MetadataNames.Aliases);

                // If this reference came from a project reference, count it as such. We never want to directly look
                // at the ProjectReference items in the project, as those don't always create project references
                // if things like OutputItemType or ReferenceOutputAssembly are set. It's also possible that other
                // MSBuild logic is adding or removing properties too.
                if (StringComparer.OrdinalIgnoreCase.Equals(referenceSourceTarget, ItemNames.ProjectReference))
                {
                    var projectReferenceOriginalItemSpec = referencePathItem.GetMetadataValue(MetadataNames.ProjectReferenceOriginalItemSpec);
                    if (projectReferenceOriginalItemSpec.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    {
                        var projectReferenceFilePath = Path.GetFullPath(Path.Combine(projectFolderPath, projectReferenceOriginalItemSpec));

                        projectReferences.Add(projectReferenceFilePath);

                        if (!string.IsNullOrEmpty(aliases))
                        {
                            projectReferenceAliases[projectReferenceFilePath] = aliases;
                        }

                        continue;
                    }
                }

                var fullPath = referencePathItem.GetMetadataValue(MetadataNames.FullPath);
                if (!string.IsNullOrEmpty(fullPath))
                {
                    references.Add(fullPath);

                    if (!string.IsNullOrEmpty(aliases))
                    {
                        referenceAliases[fullPath] = aliases;
                    }
                }
            }

            var packageReferences = GetPackageReferences(projectInstance.GetItems(ItemNames.PackageReference));
            var analyzers = GetFullPaths(projectInstance.GetItems(ItemNames.Analyzer));
            var additionalFiles = GetFullPaths(projectInstance.GetItems(ItemNames.AdditionalFiles));
            var editorConfigFiles = GetFullPaths(projectInstance.GetItems(ItemNames.EditorConfigFiles));
            var includeGlobs = project.GetAllGlobs().Select(x => x.MsBuildGlob).ToImmutableArray();

            return new ProjectData(
                guid,
                name,
                assemblyName,
                targetPath,
                outputPath,
                intermediateOutputPath,
                projectAssetsFile,
                configuration,
                platform,
                platformTarget,
                targetFramework,
                targetFrameworks,
                outputKind,
                languageVersion,
                nullableContextOptions,
                allowUnsafeCode,
                checkForOverflowUnderflow,
                documentationFile,
                preprocessorSymbolNames,
                suppressedDiagnosticIds,
                warningsAsErrors,
                warningsNotAsErrors,
                signAssembly,
                assemblyOriginatorKeyFile,
                sourceFiles,
                projectReferences.ToImmutable(),
                references.ToImmutable(),
                packageReferences,
                analyzers,
                additionalFiles,
                editorConfigFiles,
                treatWarningsAsErrors,
                defaultNamespace,
                runAnalyzers,
                runAnalyzersDuringLiveAnalysis,
                ruleset,
                referenceAliases.ToImmutableDictionary(),
                projectReferenceAliases.ToImmutable(),
                includeGlobs);
        }

        private static RuleSet ResolveRulesetIfAny(MSB.Execution.ProjectInstance projectInstance)
        {
            var rulesetIfAny = projectInstance.Properties.FirstOrDefault(x => x.Name == "ResolvedCodeAnalysisRuleSet");

            if (rulesetIfAny != null)
                return RuleSet.LoadEffectiveRuleSetFromFile(Path.Combine(projectInstance.Directory, rulesetIfAny.EvaluatedValue));

            return null;
        }

        private static bool FileNameIsNotGenerated(string filePath)
            => !Path.GetFileName(filePath).StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase);

        private static ImmutableArray<string> GetFullPaths(IEnumerable<MSB.Execution.ProjectItemInstance> items, Func<string, bool> filter = null)
        {
            var builder = ImmutableArray.CreateBuilder<string>();
            var addedSet = new HashSet<string>();

            filter = filter ?? (_ => true);

            foreach (var item in items)
            {
                var fullPath = item.GetMetadataValue(MetadataNames.FullPath);

                if (filter(fullPath) && addedSet.Add(fullPath))
                {
                    builder.Add(fullPath);
                }
            }

            return builder.ToImmutable();
        }

        private static ImmutableArray<PackageReference> GetPackageReferences(ICollection<MSB.Execution.ProjectItemInstance> items)
        {
            var builder = ImmutableArray.CreateBuilder<PackageReference>(items.Count);
            var addedSet = new HashSet<PackageReference>();

            foreach (var item in items)
            {
                var name = item.EvaluatedInclude;
                var versionValue = item.GetMetadataValue(MetadataNames.Version);
                var versionRange = PropertyConverter.ToVersionRange(versionValue);
                var dependency = new PackageDependency(name, versionRange);

                var isImplicitlyDefinedValue = item.GetMetadataValue(MetadataNames.IsImplicitlyDefined);
                var isImplicitlyDefined = PropertyConverter.ToBoolean(isImplicitlyDefinedValue, defaultValue: false);

                var packageReference = new PackageReference(dependency, isImplicitlyDefined);

                if (addedSet.Add(packageReference))
                {
                    builder.Add(packageReference);
                }
            }

            return builder.ToImmutable();
        }
    }
}