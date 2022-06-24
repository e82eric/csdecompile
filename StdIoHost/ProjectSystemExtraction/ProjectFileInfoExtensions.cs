using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

internal static class ProjectFileInfoExtensions
{
    public static CSharpCompilationOptions CreateCompilationOptions(this ProjectFileInfo projectFileInfo)
    {
        var compilationOptions = new CSharpCompilationOptions(projectFileInfo.OutputKind);
        return projectFileInfo.CreateCompilationOptions(compilationOptions);
    }

    public static CSharpCompilationOptions CreateCompilationOptions(this ProjectFileInfo projectFileInfo, CSharpCompilationOptions existingCompilationOptions)
    {
        var compilationOptions = existingCompilationOptions.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default)
            .WithSpecificDiagnosticOptions(projectFileInfo.GetDiagnosticOptions())
            .WithOverflowChecks(projectFileInfo.CheckForOverflowUnderflow);

        var platformTarget = ParsePlatform(projectFileInfo.PlatformTarget);
        if (platformTarget != compilationOptions.Platform)
        {
            compilationOptions = compilationOptions.WithPlatform(platformTarget);
        }

        if (projectFileInfo.AllowUnsafeCode != compilationOptions.AllowUnsafe)
        {
            compilationOptions = compilationOptions.WithAllowUnsafe(projectFileInfo.AllowUnsafeCode);
        }

        compilationOptions = projectFileInfo.TreatWarningsAsErrors ?
            compilationOptions.WithGeneralDiagnosticOption(ReportDiagnostic.Error) : compilationOptions.WithGeneralDiagnosticOption(ReportDiagnostic.Default);

        if (projectFileInfo.NullableContextOptions != compilationOptions.NullableContextOptions)
        {
            compilationOptions = compilationOptions.WithNullableContextOptions(projectFileInfo.NullableContextOptions);
        }

        if (projectFileInfo.SignAssembly && !string.IsNullOrEmpty(projectFileInfo.AssemblyOriginatorKeyFile))
        {
            var keyFile = Path.Combine(projectFileInfo.Directory, projectFileInfo.AssemblyOriginatorKeyFile);
            compilationOptions = compilationOptions.WithStrongNameProvider(new DesktopStrongNameProvider())
                .WithCryptoKeyFile(keyFile);
        }

        if (!string.IsNullOrWhiteSpace(projectFileInfo.DocumentationFile))
        {
            compilationOptions = compilationOptions.WithXmlReferenceResolver(XmlFileResolver.Default);
        }

        return compilationOptions;
    }

    public static ImmutableDictionary<string, ReportDiagnostic> GetDiagnosticOptions(this ProjectFileInfo projectFileInfo)
    {
        var suppressions = CompilationOptionsHelper.GetDefaultSuppressedDiagnosticOptions(projectFileInfo.SuppressedDiagnosticIds);
        var specificRules = projectFileInfo.RuleSet?.SpecificDiagnosticOptions ?? ImmutableDictionary<string, ReportDiagnostic>.Empty;

        // suppressions capture NoWarn and they have the highest priority
        var combinedRules = specificRules.Concat(suppressions.Where(x => !specificRules.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);

        // then handle WarningsAsErrors
        foreach (var warningAsError in projectFileInfo.WarningsAsErrors)
        {
            if (!suppressions.ContainsKey(warningAsError))
            {
                combinedRules[warningAsError] = ReportDiagnostic.Error;
            }
        }

        // WarningsNotAsErrors can overwrite WarningsAsErrors
        foreach (var warningNotAsError in projectFileInfo.WarningsNotAsErrors)
        {
            if (!suppressions.ContainsKey(warningNotAsError))
            {
                combinedRules[warningNotAsError] = ReportDiagnostic.Warn;
            }
        }

        return combinedRules.ToImmutableDictionary();
    }

    public static ProjectInfo CreateProjectInfo(this ProjectFileInfo projectFileInfo, IAnalyzerAssemblyLoader analyzerAssemblyLoader)
    {
        var analyzerReferences = projectFileInfo.ResolveAnalyzerReferencesForProject(analyzerAssemblyLoader);

        return ProjectInfo.Create(
            id: projectFileInfo.Id,
            version: VersionStamp.Create(),
            name: projectFileInfo.Name,
            assemblyName: projectFileInfo.AssemblyName,
            language: LanguageNames.CSharp,
            filePath: projectFileInfo.FilePath,
            outputFilePath: projectFileInfo.TargetPath,
            compilationOptions: projectFileInfo.CreateCompilationOptions(),
            analyzerReferences: analyzerReferences).WithDefaultNamespace(projectFileInfo.DefaultNamespace);
    }

    public static ImmutableArray<AnalyzerFileReference> ResolveAnalyzerReferencesForProject(this ProjectFileInfo projectFileInfo, IAnalyzerAssemblyLoader analyzerAssemblyLoader)
    {
        if (!projectFileInfo.RunAnalyzers || !projectFileInfo.RunAnalyzersDuringLiveAnalysis)
        {
            return ImmutableArray<AnalyzerFileReference>.Empty;
        }

        foreach(var analyzerAssemblyPath in projectFileInfo.Analyzers.Distinct())
        {
            analyzerAssemblyLoader.AddDependencyLocation(analyzerAssemblyPath);
        }

        return projectFileInfo.Analyzers.Select(analyzerCandicatePath => new AnalyzerFileReference(analyzerCandicatePath, analyzerAssemblyLoader)).ToImmutableArray();
    }

    private static Microsoft.CodeAnalysis.Platform ParsePlatform(string value) => (value?.ToLowerInvariant()) switch
    {
        "x86" => Microsoft.CodeAnalysis.Platform.X86,
        "x64" => Microsoft.CodeAnalysis.Platform.X64,
        "itanium" => Microsoft.CodeAnalysis.Platform.Itanium,
        "anycpu" => Microsoft.CodeAnalysis.Platform.AnyCpu,
        "anycpu32bitpreferred" => Microsoft.CodeAnalysis.Platform.AnyCpu32BitPreferred,
        "arm" => Microsoft.CodeAnalysis.Platform.Arm,
        "arm64" => Microsoft.CodeAnalysis.Platform.Arm64,
        _ => Microsoft.CodeAnalysis.Platform.AnyCpu,
    };
}