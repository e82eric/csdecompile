﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OmniSharp.MSBuild;
using OmniSharp.Roslyn.Utilities;
using TryOmnisharpExtension;
using TryOmnisharpExtension.FindImplementations;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.GetMembers;
using TryOmnisharpExtension.GetSource;
using TryOmnisharpExtension.GotoDefinition;
using TryOmnisharpExtension.IlSpy;

namespace StdIoHost.ProjectSystemExtraction;

internal static class OmniSharpApplication
{
    private static OmniSharpWorkspace2 _workspace;
    private static ILoggerFactory _loggerFactory;
    private static PeFileCache _peFileCache;
    private static IlSpyTypeSystemFactory _decompilerTypeSystemFactory;
    private static DecompileWorkspace _decompileWorkspace;
    private static TextReader _stdIn;
    private static SharedTextWriter _sharedTextWriter;

    public static async Task Init(TextWriter stdOut, TextReader stdIn, string solutionPath)
    {
        _stdIn = stdIn;
        _sharedTextWriter = new SharedTextWriter(stdOut);
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddConsole().AddStdio(_sharedTextWriter);
        });
        ILogger logger = _loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Info Log");
        logger.LogWarning("Warning Log");
        logger.LogError("Error Log");
        logger.LogCritical("Critical Log");

        _workspace = GetWorkspace(_loggerFactory);
        
        _peFileCache = new PeFileCache();
        var resolverFactory = new AssemblyResolverFactory(_peFileCache);
        _decompilerTypeSystemFactory = new IlSpyTypeSystemFactory(resolverFactory, _peFileCache);
        _decompileWorkspace = new DecompileWorkspace(_workspace, _peFileCache);
        var eventEmitter = new StdioEventEmitter(_sharedTextWriter);
        var solutionFileInfo = new FileInfo(solutionPath);
        var targetDirectory = solutionFileInfo.Directory.FullName;
        var projectSystem = GetProjectSystem(eventEmitter, targetDirectory);
        await projectSystem.Start(LogLevel.Debug, solutionFileInfo);
    }

    private static OmniSharpWorkspace2 GetWorkspace(ILoggerFactory loggerFactory)
    {
        var hostAggregator = new HostServicesAggregator(new IHostServicesProvider[] { }, loggerFactory);
        var workspace = new OmniSharpWorkspace2(hostAggregator, loggerFactory);
        return workspace;
    }

    public static ProjectSystem GetProjectSystem(IEventEmitter eventEmitter, string targetDirectory)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var analyzer = ShadowCopyAnalyzerAssemblyLoader.Instance;
        var dotnetCliService = new DotNetCliService(_loggerFactory, eventEmitter, targetDirectory);
        var dotNetInfo = dotnetCliService.GetInfo(targetDirectory);
        var metadataFileReferenceCache = new MetadataFileReferenceCache(memoryCache, _loggerFactory);
        var sdksPathResolver = new SdksPathResolver(dotnetCliService, _loggerFactory);
        var msBuildLocator = MSBuildLocator.CreateDefault(_loggerFactory);
        msBuildLocator.RegisterDefaultInstance(_loggerFactory.CreateLogger("Stuf"));
        
        var msBuildOptions = new MSBuildOptions();

        sdksPathResolver.Enabled = msBuildOptions.UseLegacySdkResolver;
        sdksPathResolver.OverridePath = msBuildOptions.MSBuildSDKsPath;

        // if (logLevel < LogLevel.Information)
        // {
        //     var buildEnvironmentInfo = MSBuildHelpers.GetBuildEnvironmentInfo();
        //     _logger.LogDebug($"MSBuild environment: {Environment.NewLine}{buildEnvironmentInfo}");
        // }

        var propertyOverrides = msBuildLocator.RegisteredInstance?.PropertyOverrides ?? ImmutableDictionary.Create<string, string>();
        var packageDependencyChecker = new PackageDependencyChecker(
            _loggerFactory,
            eventEmitter,
            dotnetCliService,
            msBuildOptions);
        
        var projectLoader = new ProjectLoader(msBuildOptions,
            targetDirectory,
            propertyOverrides,
            _loggerFactory,
            sdksPathResolver);

        var projectManager = new ProjectManager(
            _loggerFactory,
            // msBuildOptions,
            eventEmitter,
            metadataFileReferenceCache,
            packageDependencyChecker,
            projectLoader,
            _workspace,
            analyzer,
            // eventSinks.ToImmutableArray(),
            dotNetInfo);

        var system = new ProjectSystem(
            eventEmitter,
            _loggerFactory,
            _decompileWorkspace,
            projectManager);
        
        return system;
    }

    public static DecompileGotoDefinitionHandler CreateGoToDefinitionHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory, decompilerFactory);
        var typeInTypeFinder = new TypeInTypeFinder();
        var ilSpyTypeFinder = new IlSpyTypeFinder(typeInTypeFinder, decompilerFactory);
        var methodInTypeFinder = new MethodInTypeFinder();
        var ilSpyMemberFinder = new IlSpyMemberFinder(methodInTypeFinder, decompilerFactory);
        var propertyInTypeFinder = new PropertyInTypeFinder();
        var ilSpyPropertyFinder = new IlSpyPropertyFinder(propertyInTypeFinder, decompilerFactory);
        var eventInTypeFinder = new EventInTypeFinder();
        var ilSpyEventFinder = new IlSpyEventFinder(eventInTypeFinder, decompilerFactory);
        var fieldInTypeFinder = new FieldInTypeFinder();
        var ilSpyFieldFinder = new IlSpyFieldFinder(fieldInTypeFinder, decompilerFactory);
        var gotoDefinitionCommandFactory = new GotoDefinitionCommandFactory(
            ilSpyTypeFinder, ilSpyMemberFinder, ilSpyPropertyFinder, ilSpyEventFinder, ilSpyFieldFinder);
        var roslynSymbolInfoFinder = new RosylnSymbolInfoFinder<IGotoDefinitionCommand>(
            _workspace,
            ilSpySymbolFinder,
            gotoDefinitionCommandFactory);
        var ilSpySymbolInfoFinder = new IlSpyCommandFactory<IGotoDefinitionCommand>(
            ilSpySymbolFinder,
            gotoDefinitionCommandFactory);
        var extensionContainer = new ExtensionContainer();
        var result = new DecompileGotoDefinitionHandler(
            roslynSymbolInfoFinder,
            ilSpySymbolInfoFinder,
            extensionContainer);
        return result;
    }

    public static DecompiledSourceHandler CreateDecompiledSourceHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory, decompilerFactory);
        var ilSpyDecompiledSourceCommandFactory = new IlSpyDecompiledSourceCommandFactory(decompilerFactory, ilSpySymbolFinder);
        var extensionContainer = new ExtensionContainer();
        var result = new DecompiledSourceHandler(ilSpyDecompiledSourceCommandFactory, extensionContainer);
        return result;
    }

    public static DecompileFindImplementationsHandler CreateFindImplementationsHandler()
    {
        var assemblyResolverFactory = new AssemblyResolverFactory(_peFileCache);
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory, decompilerFactory);
        var analyzerScope = new AnalyzerScope(assemblyResolverFactory, _decompileWorkspace);
        var typesThatUseTypeAsBaseTypeMetadataScanner = new TypesThatUseTypeAsBaseTypeMetadataScanner(analyzerScope);
        var typeUsedAsBaseTypeFinder = new TypeUsedAsBaseTypeFinder();
        var ilSpyBaseTypeUsageFinder = new IlSpyBaseTypeUsageFinder(
            typesThatUseTypeAsBaseTypeMetadataScanner,
            typeUsedAsBaseTypeFinder,
            decompilerFactory);
        var typesThatUseMemberAsBaseTypeMetadataScanner = new TypesThatUseMemberAsBaseTypeMetadataScanner(analyzerScope);
        var memberOverrideInTypeFinder = new MemberOverrideInTypeFinder();
        var ilSpyMemberImplementationFinder = new IlSpyMemberImplementationFinder(
            decompilerFactory,
            typesThatUseMemberAsBaseTypeMetadataScanner,
            memberOverrideInTypeFinder);
        var ilSpyFindImplementationsCommandFactoryTemp = new IlSpyFindImplementationsCommandFactoryTemp(
            ilSpyBaseTypeUsageFinder, ilSpyMemberImplementationFinder, _workspace);
        var everywhereSymbolInfoFinder2 = new EverywhereSymbolInfoFinder2<FindImplementationsResponse>(
            _workspace, ilSpySymbolFinder, ilSpyFindImplementationsCommandFactoryTemp);
        var ilSpyFindImplementationsCommandFactory2 = new IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse>(
            ilSpySymbolFinder,
            ilSpyFindImplementationsCommandFactoryTemp,
            _decompileWorkspace);
        var extensionContainer = new ExtensionContainer();
        var result = new DecompileFindImplementationsHandler(everywhereSymbolInfoFinder2,
            ilSpyFindImplementationsCommandFactory2, extensionContainer);
        return result;
    }
    public static DecompileFindUsagesHandler CreateFindUsagesHandler()
    {
        var assemblyResolverFactory = new AssemblyResolverFactory(_peFileCache);
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory, decompilerFactory);
        var analyzerScope = new AnalyzerScope(assemblyResolverFactory, _decompileWorkspace);
        var typeUsedByTypeIlScanner = new TypeUsedByTypeIlScanner(analyzerScope);
        var typeUsedInTypeFinder3 = new TypeUsedInTypeFinder3();
        var ilSpyTypeUsagesFinder = new IlSpyTypeUsagesFinder(
            decompilerFactory,
            typeUsedByTypeIlScanner,
            typeUsedInTypeFinder3);
        var methodUsedByMetadataScanner = new MethodUsedByMetadataScanner(analyzerScope);
        var inMemberBodyFinder = new MemberUsedInTypeFinder();
        var ilSpyMethodUsagesFinder = new IlSpyMethodUsagesFinder(
            decompilerFactory,
            methodUsedByMetadataScanner,
            inMemberBodyFinder);
        var propertyUsedByMetadataScanner = new PropertyUsedByMetadataScanner(analyzerScope);
        var ilSpyPropertyUsagesFinder = new IlSpyPropertyUsagesFinder(
            propertyUsedByMetadataScanner,
            inMemberBodyFinder,
            decompilerFactory);
        var fieldUsedByMetadataScanner = new FieldUsedByMetadataScanner(analyzerScope);
        var ilSpyFieldUsagesFinder = new IlSpyFieldUsagesFinder(
            fieldUsedByMetadataScanner,
            inMemberBodyFinder,
            decompilerFactory);
        var variableInMethodBodyFinder = new VariableInMethodBodyFinder();
        var ilSpyVariableUsagesFinder = new IlSpyVariableUsagesFinder(variableInMethodBodyFinder);
        var eventUsedByMetadataScanner = new EventUsedByMetadataScanner(analyzerScope);
        var ilSpyEventUsagesFinder = new IlSpyEventUsagesFinder(
            decompilerFactory,
            eventUsedByMetadataScanner,
            inMemberBodyFinder);
        var findUsagesCommandFactory = new FindUsagesCommandFactory(
            ilSpyTypeUsagesFinder,
            ilSpyMethodUsagesFinder,
            ilSpyPropertyUsagesFinder,
            ilSpyFieldUsagesFinder,
            ilSpyVariableUsagesFinder,
            ilSpyEventUsagesFinder,
            _workspace);
        var everywhereSymbolInfoFinder2 = new EverywhereSymbolInfoFinder2<FindUsagesResponse>(
            _workspace,
            ilSpySymbolFinder,
            findUsagesCommandFactory);
        var ilSpyFindImplementationsCommandFactory2 = new IlSpyFindImplementationsCommandFactory2<FindUsagesResponse>(
            ilSpySymbolFinder,
            findUsagesCommandFactory,
            _decompileWorkspace);
        var extensionContainer = new ExtensionContainer();
        var result = new DecompileFindUsagesHandler(
            everywhereSymbolInfoFinder2,
            ilSpyFindImplementationsCommandFactory2,
            extensionContainer);
        return result;
    }

    public static GetTypesHandler CreateGetTypesHandlers()
    {
        var assemblyResolverFactory = new AssemblyResolverFactory(_peFileCache);
        var allTypesRepository = new AllTypesRepository(_decompileWorkspace, assemblyResolverFactory);
        var result = new GetTypesHandler(allTypesRepository);
        return result;
    }
    
    public static GetTypeMembersHandler CreateGetTypeMembersHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory, decompilerFactory);
        var typeMembersFinder = new TypeMembersFinder();
        var ilSpyTypeMembersFinder = new IlSpyTypeMembersFinder(typeMembersFinder, decompilerFactory);
        var ilSpyGetMembersCommandFactory = new IlSpyGetMembersCommandFactory(ilSpySymbolFinder, ilSpyTypeMembersFinder);
        var roslynGetTypeMembersCommandFactory = new RoslynGetTypeMembersCommandFactory(_workspace);
        var result = new GetTypeMembersHandler(ilSpyGetMembersCommandFactory, roslynGetTypeMembersCommandFactory);
        return result;
    }

    public static Host CreateHost()
    {
        var gotoDefinitionHandler = CreateGoToDefinitionHandler();
        var decompileFindImplementationsHandler = CreateFindImplementationsHandler();
        var decompiledSourceHandler = CreateDecompiledSourceHandler();
        var decompileFindUsagesHandler = CreateFindUsagesHandler();
        var getTypesHandlers = CreateGetTypesHandlers();
        var getTypeMembersHandler = CreateGetTypeMembersHandler();
        var router = new Router(
            gotoDefinitionHandler,
            decompileFindImplementationsHandler,
            decompiledSourceHandler,
            decompileFindUsagesHandler,
            getTypesHandlers,
            getTypeMembersHandler);
        
        
        var result = new Host(_stdIn, _sharedTextWriter, router);
        return result;
    }
}