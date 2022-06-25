using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Build.Globbing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
    // public ImmutableArray<string> AdditionalFiles => _data.AdditionalFiles;
    // public ImmutableArray<string> AnalyzerConfigFiles => _data.AnalyzerConfigFiles;
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

    public ProjectFileInfo(
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

    internal static ProjectFileInfo CreateNoBuild(string filePath, ProjectLoader loader, DotNetInfo dotNetInfo)
    {
        var id = ProjectId.CreateNewId(debugName: filePath);
        var project = loader.EvaluateProjectFile(filePath);

        var data = ProjectData.Create(project);
        //we are not reading the solution here
        var projectIdInfo = new ProjectIdInfo(id, isDefinedInSolution: false);

        return new ProjectFileInfo(projectIdInfo, filePath, data, sessionId: Guid.NewGuid(), dotNetInfo);
    }

    public bool IsUnityProject()
        => References.Any(filePath =>
        {
            var fileName = Path.GetFileName(filePath);

            return string.Equals(fileName, "UnityEngine.dll", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(fileName, "UnityEditor.dll", StringComparison.OrdinalIgnoreCase);
        });
}