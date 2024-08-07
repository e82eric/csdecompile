using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using StdIoHost.ProjectSystemExtraction;
using StdIoHost.SimpleProjectSystem;
using CsDecompileLib;
using CsDecompileLib.DebugClientAssemblySource;
using CsDecompileLib.ExternalAssemblies;
using CsDecompileLib.FindImplementations;
using CsDecompileLib.FindUsages;
using CsDecompileLib.GetMembers;
using CsDecompileLib.GetSource;
using CsDecompileLib.GetSymbolInfo;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;
using CsDecompileLib.IlSpy.Ast;
using CsDecompileLib.Nuget;
using CsDecompileLib.Roslyn;

namespace StdIoHost;

internal static class HandlerFactory
{
    private static ICsDecompileWorkspace _workspace;
    // private static ILoggerFactory _loggerFactory;
    private static PeFileCache _peFileCache;
    private static IlSpyTypeSystemFactory _decompilerTypeSystemFactory;
    private static IDecompileWorkspace _decompileWorkspace;
    private static TextReader _stdIn;
    private static SharedTextWriter _sharedTextWriter;
    private static DataTargetProvider _dataTargetProvider;

    public static async Task Init(TextWriter stdOut, TextReader stdIn, string solutionPath)
    {
        _stdIn = stdIn;
        _sharedTextWriter = new SharedTextWriter(stdOut);
        // _loggerFactory = LoggerFactory.Create(builder =>
        // {
        //     builder
        //         .AddFilter("Microsoft", LogLevel.Warning)
        //         .AddFilter("System", LogLevel.Warning)
        //         .AddConsole().AddStdio(_sharedTextWriter);
        // });

        var eventEmitter = new StdioEventEmitter(_sharedTextWriter);
        _workspace = GetWorkspace(eventEmitter);
        
        _peFileCache = new PeFileCache();
        var resolverFactory = new AssemblyResolverFactory(_peFileCache);
        _decompilerTypeSystemFactory = new IlSpyTypeSystemFactory(resolverFactory, _peFileCache);
        var decompileWorkspace = new DecompileWorkspace(_workspace, _peFileCache);
        _decompileWorkspace = decompileWorkspace;

        var solutionFileInfo = new FileInfo(solutionPath);
        var projectsLoadTimer = Stopwatch.StartNew();
        await ((SimpleDecompileWorkspace)_workspace).Start(solutionFileInfo);
        projectsLoadTimer.Stop();
        
        var dllLoadTimer = Stopwatch.StartNew();
        var assemblyCount = decompileWorkspace.LoadDlls();
        dllLoadTimer.Stop();
        var compilationsTimer = Stopwatch.StartNew();
        await decompileWorkspace.RunProjectCompilations();
        compilationsTimer.Stop();
        eventEmitter.Emit("ASSEMBLIES_LOADED", new
        {
            AssembliesLoaded = assemblyCount,
            ProjectsLoadTime = projectsLoadTimer.Elapsed,
            DllLoadTime = dllLoadTimer.Elapsed,
            CompilationsTime = compilationsTimer.Elapsed
        });
    }
    
    public static Task InitNoSolution(TextWriter stdOut, TextReader stdIn)
    {
        _stdIn = stdIn;
        _sharedTextWriter = new SharedTextWriter(stdOut);
        // _loggerFactory = LoggerFactory.Create(builder =>
        // {
        //     builder
        //         .AddFilter("Microsoft", LogLevel.Warning)
        //         .AddFilter("System", LogLevel.Warning)
        //         .AddConsole().AddStdio(_sharedTextWriter);
        // });

        var eventEmitter = new StdioEventEmitter(_sharedTextWriter);
        _workspace = GetWorkspace(eventEmitter);
        
        _peFileCache = new PeFileCache();
        var resolverFactory = new AssemblyResolverFactory(_peFileCache);
        _decompilerTypeSystemFactory = new IlSpyTypeSystemFactory(resolverFactory, _peFileCache);
        _decompileWorkspace = new NoSolutionDecompileWorkspace(_peFileCache, _decompilerTypeSystemFactory);
        _dataTargetProvider = new DataTargetProvider();
        return Task.FromResult(0);
    }

    public static async Task InitFromMemoryDump(TextWriter stdOut, TextReader stdIn, string memoryDumpPath)
    {
        await InitNoSolution(stdOut, stdIn);
        var memoryDumpLoader = GetMemoryDumpLoader();
        memoryDumpLoader.LoadDllsFromMemoryDump(memoryDumpPath);
    }
    
    public static async Task InitFromProcess(TextWriter stdOut, TextReader stdIn, int processId)
    {
        await InitNoSolution(stdOut, stdIn);
        var memoryDumpLoader = GetMemoryDumpLoader();
        memoryDumpLoader.LoadAssembliesFromProcess(processId, true);
    }

    public static MemoryDumpLoader GetMemoryDumpLoader()
    {
        var result = new MemoryDumpLoader(_decompileWorkspace, new ClrMdDllExtractor(), _dataTargetProvider);
        return result;
    }

    private static ICsDecompileWorkspace GetWorkspace(StdioEventEmitter eventEmitter)
    {
        var result = new SimpleDecompileWorkspace(eventEmitter);
        return result;
    }

    public static NavigationHandlerBase<DecompiledLocationRequest, SymbolInfo> CreateGetSymbolInfoHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var getSymbolInfoCommandFactory = new GetSymbolInfoCommandFactory(_decompileWorkspace);
        var roslynLocationToCommandFactory = new RoslynLocationCommandFactory<INavigationCommand<SymbolInfo>>(
            _workspace,
            ilSpySymbolFinder,
            getSymbolInfoCommandFactory);
        var classLevelVariableCommandProvider =
            new ClassLevelVariableCommandProvider<INavigationCommand<SymbolInfo>>(
                ilSpySymbolFinder,
                getSymbolInfoCommandFactory,
                decompilerFactory);
        var assemblyLevelVariableCommandProvider =
            new AssemblyLevelVariableCommandProvider<INavigationCommand<SymbolInfo>>(
                decompilerFactory,
                new MemberNodeInTypeAstFinder(),
                ilSpySymbolFinder,
                getSymbolInfoCommandFactory);
        var classLevelCommandFactory = new IlSpyCommandFactory<INavigationCommand<SymbolInfo>>(
            classLevelVariableCommandProvider,
            getSymbolInfoCommandFactory,
            ilSpySymbolFinder);
        var assemblyLevelCommandFactory = new IlSpyCommandFactory<INavigationCommand<SymbolInfo>>(
            assemblyLevelVariableCommandProvider,
            getSymbolInfoCommandFactory,
            ilSpySymbolFinder);
        var result = new NavigationHandlerBase<DecompiledLocationRequest, SymbolInfo>(
            roslynLocationToCommandFactory,
            classLevelCommandFactory,
            assemblyLevelCommandFactory);
        return result;
    }

    public static NavigationHandlerBase<DecompiledLocationRequest, LocationsResponse> CreateGoToDefinitionHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var typeInTypeFinder = new TypeNodeInTypeAstFinder();
        var ilSpyTypeFinder = new IlSpyDefinitionFinderBase<ITypeDefinition>(typeInTypeFinder, typeInTypeFinder, decompilerFactory);
        var methodInTypeFinder = new MethodNodeInTypeAstFinder();
        var ilSpyMemberFinder = new IlSpyDefinitionFinderBase<IMethod>(methodInTypeFinder, typeInTypeFinder, decompilerFactory);
        var propertyInTypeFinder = new PropertyNodeInTypeAstFinder();
        var ilSpyPropertyFinder = new IlSpyDefinitionFinderBase<IProperty>(propertyInTypeFinder, typeInTypeFinder, decompilerFactory);
        var eventInTypeFinder = new EventNodeInTypeAstFinder();
        var ilSpyEventFinder = new IlSpyDefinitionFinderBase<IEvent>(eventInTypeFinder, typeInTypeFinder, decompilerFactory);
        var fieldInTypeFinder = new FieldNodeInTypeAstFinder();
        var ilSpyFieldFinder = new IlSpyDefinitionFinderBase<IField>(fieldInTypeFinder, typeInTypeFinder, decompilerFactory);
        var gotoDefinitionCommandFactory = new GotoDefinitionCommandFactory(
            ilSpyTypeFinder,
            ilSpyMemberFinder,
            ilSpyPropertyFinder,
            ilSpyEventFinder,
            ilSpyFieldFinder,
            GetAllTypesRepository2(),
            new RoslynAllTypesRepository(_workspace),
            _workspace);
        var roslynSymbolInfoFinder = new RoslynLocationCommandFactory<INavigationCommand<LocationsResponse>>(
            _workspace,
            ilSpySymbolFinder,
            gotoDefinitionCommandFactory);
        var memberInTypeFinder = new MemberNodeInTypeAstFinder();
        var assemblyLevelVariableCommandProvider = new AssemblyLevelVariableCommandProvider<INavigationCommand<LocationsResponse>>(
            decompilerFactory,
            memberInTypeFinder,
            ilSpySymbolFinder,
            gotoDefinitionCommandFactory);
        var classLevelVariableCommandProvider = new ClassLevelVariableCommandProvider<INavigationCommand<LocationsResponse>>(
            ilSpySymbolFinder,
            gotoDefinitionCommandFactory,
            decompilerFactory);
        var classLevelCommandFactory = new IlSpyCommandFactory<INavigationCommand<LocationsResponse>>(
            classLevelVariableCommandProvider,
            gotoDefinitionCommandFactory,
            ilSpySymbolFinder);
        var assemblyLevelCommandFactory = new IlSpyCommandFactory<INavigationCommand<LocationsResponse>>(
            assemblyLevelVariableCommandProvider,
            gotoDefinitionCommandFactory,
            ilSpySymbolFinder);
        var result = new NavigationHandlerBase<DecompiledLocationRequest, LocationsResponse>(
            roslynSymbolInfoFinder,
            classLevelCommandFactory,
            assemblyLevelCommandFactory);
        return result;
    }

    public static DecompiledSourceHandler CreateDecompiledSourceHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var ilSpyDecompiledSourceCommandFactory = new IlSpyDecompiledSourceCommandFactory(decompilerFactory, ilSpySymbolFinder);
        var result = new DecompiledSourceHandler(ilSpyDecompiledSourceCommandFactory);
        return result;
    }

    public static NavigationHandlerBase<DecompiledLocationRequest, LocationsResponse> CreateFindImplementationsHandler()
    {
        var assemblyResolverFactory = new AssemblyResolverFactory(_peFileCache);
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var analyzerScope = new AnalyzerScope(assemblyResolverFactory, _decompileWorkspace);
        var typesThatUseTypeAsBaseTypeMetadataScanner = new TypesThatUseTypeAsBaseTypeMetadataScanner(analyzerScope);
        var typeUsedAsBaseTypeFinder = new TypeUsedAsBaseTypeFinder();
        var ilSpyBaseTypeUsageFinder = new IlSpyUsagesFinderBase<ITypeDefinition>(
            decompilerFactory,
            typesThatUseTypeAsBaseTypeMetadataScanner,
            typeUsedAsBaseTypeFinder);
        var typesThatUseMemberAsBaseTypeMetadataScanner = new TypesThatUseMemberAsBaseTypeMetadataScanner(analyzerScope);
        var memberOverrideInTypeFinder = new MemberOverrideInTypeFinder();
        var ilSpyMemberImplementationFinder = new IlSpyUsagesFinderBase<IMember>(
            decompilerFactory,
            typesThatUseMemberAsBaseTypeMetadataScanner,
            memberOverrideInTypeFinder);
        var ilSpyFindImplementationsCommandFactoryTemp = new RoslynFindImplementationsCommandFactory(
            ilSpyBaseTypeUsageFinder, ilSpyMemberImplementationFinder, _workspace);
        var everywhereSymbolInfoFinder2 = new EverywhereSymbolInfoFinder<LocationsResponse>(
            _workspace, ilSpySymbolFinder, ilSpyFindImplementationsCommandFactoryTemp);
        var memberInTypeFinder = new MemberNodeInTypeAstFinder();
        var assemblyLevelVariableCommandProvider = new AssemblyLevelVariableCommandProvider<INavigationCommand<LocationsResponse>>(
            decompilerFactory,
            memberInTypeFinder,
            ilSpySymbolFinder,
            ilSpyFindImplementationsCommandFactoryTemp);
        var classLevelVariableCommandProvider = new ClassLevelVariableCommandProvider<INavigationCommand<LocationsResponse>>(
            ilSpySymbolFinder,
            ilSpyFindImplementationsCommandFactoryTemp,
            decompilerFactory);
        var everyWhereFindImplementationsCommandFactory = new EveryWhereFindImplementationsCommandFactory<LocationsResponse>(
            ilSpyFindImplementationsCommandFactoryTemp,
            _decompileWorkspace);
        var classLevelFindImplementationsCommandFactory = new IlSpyCommandFactory<INavigationCommand<LocationsResponse>>(
            classLevelVariableCommandProvider,
            everyWhereFindImplementationsCommandFactory,
            ilSpySymbolFinder);
        var assemblyLevelFindImplementationsCommandFactory = new IlSpyCommandFactory<INavigationCommand<LocationsResponse>>(
            assemblyLevelVariableCommandProvider,
            everyWhereFindImplementationsCommandFactory,
            ilSpySymbolFinder);
        var result = new NavigationHandlerBase<DecompiledLocationRequest, LocationsResponse>(
            everywhereSymbolInfoFinder2,
            classLevelFindImplementationsCommandFactory,
            assemblyLevelFindImplementationsCommandFactory);
        return result;
    }
    public static NavigationHandlerBase<DecompiledLocationRequest, LocationsResponse> CreateFindUsagesHandler()
    {
        var assemblyResolverFactory = new AssemblyResolverFactory(_peFileCache);
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var analyzerScope = new AnalyzerScope(assemblyResolverFactory, _decompileWorkspace);
        var typeUsedByTypeIlScanner = new TypeUsedByTypeIlScanner(analyzerScope);
        var typeUsedInTypeFinder3 = new TypeUsedInTypeFinder();
        var ilSpyTypeUsagesFinder = new IlSpyUsagesFinderBase<ITypeDefinition>(
            decompilerFactory,
            typeUsedByTypeIlScanner,
            typeUsedInTypeFinder3);
        var methodUsedByMetadataScanner = new MethodUsedByMetadataScanner(analyzerScope);
        var inMemberBodyFinder = new MemberUsedInTypeFinder();
        var ilSpyMethodUsagesFinder = new IlSpyUsagesFinderBase<IMember>(
            decompilerFactory,
            methodUsedByMetadataScanner,
            inMemberBodyFinder);
        var propertyUsedByMetadataScanner = new PropertyUsedByMetadataScanner(analyzerScope);
        var ilSpyPropertyUsagesFinder = new IlSpyUsagesFinderBase<IMember>(
            decompilerFactory,
            propertyUsedByMetadataScanner,
            inMemberBodyFinder);
        var fieldUsedByMetadataScanner = new FieldUsedByMetadataScanner(analyzerScope);
        var ilSpyFieldUsagesFinder = new IlSpyUsagesFinderBase<IMember>(
            decompilerFactory,
            fieldUsedByMetadataScanner,
            inMemberBodyFinder);
        var variableInMethodBodyFinder = new VariableInMethodBodyFinder();
        var ilSpyVariableUsagesFinder = new IlSpyVariableUsagesFinder(variableInMethodBodyFinder);
        var eventUsedByMetadataScanner = new EventUsedByMetadataScanner(analyzerScope);
        var ilSpyEventUsagesFinder = new IlSpyUsagesFinderBase<IMember>(
            decompilerFactory,
            eventUsedByMetadataScanner,
            inMemberBodyFinder);
        var enumMemberParentUsedByTypeIlScanner = new EnumMemberParentUsedByTypeIlScanner(
            analyzerScope,
            typeUsedByTypeIlScanner);
        var ilSpyUsagesEnumFieldFinder = new IlSpyUsagesFinderBase<IMember>(
            decompilerFactory,
            enumMemberParentUsedByTypeIlScanner,
            inMemberBodyFinder);
        var findUsagesCommandFactory = new FindUsagesCommandFactory(
            ilSpyTypeUsagesFinder,
            ilSpyMethodUsagesFinder,
            ilSpyPropertyUsagesFinder,
            ilSpyFieldUsagesFinder,
            ilSpyUsagesEnumFieldFinder,
            ilSpyVariableUsagesFinder,
            ilSpyEventUsagesFinder,
            _workspace);
        var everywhereSymbolInfoFinder2 = new EverywhereSymbolInfoFinder<LocationsResponse>(
            _workspace,
            ilSpySymbolFinder,
            findUsagesCommandFactory);
        var classLevelVariableCommandProvider = new ClassLevelVariableCommandProvider<INavigationCommand<LocationsResponse>>(
            ilSpySymbolFinder,
            findUsagesCommandFactory,
            decompilerFactory);
        var everyWhereFindImplementationsCommandFactory = new EveryWhereFindImplementationsCommandFactory<LocationsResponse>(
            findUsagesCommandFactory,
            _decompileWorkspace);
        var classLevelFindImplementationsCommandFactory = new IlSpyCommandFactory<INavigationCommand<LocationsResponse>>(
            classLevelVariableCommandProvider,
            everyWhereFindImplementationsCommandFactory,
            ilSpySymbolFinder);
        var memberInTypeFinder = new MemberNodeInTypeAstFinder();
        var assemblyLevelVariableCommandProvider = new AssemblyLevelVariableCommandProvider<INavigationCommand<LocationsResponse>>(
            decompilerFactory,
            memberInTypeFinder,
            ilSpySymbolFinder,
            findUsagesCommandFactory);
        var assemblyLevelFindImplementationsCommandFactory = new IlSpyCommandFactory<INavigationCommand<LocationsResponse>>(
            assemblyLevelVariableCommandProvider,
            everyWhereFindImplementationsCommandFactory,
            ilSpySymbolFinder);
        var result = new NavigationHandlerBase<DecompiledLocationRequest, LocationsResponse>(
            everywhereSymbolInfoFinder2,
            classLevelFindImplementationsCommandFactory,
            assemblyLevelFindImplementationsCommandFactory);
        return result;
    }

    public static GetTypesHandler CreateGetTypesHandlers()
    {
        var allTypesRepository = GetAllTypesRepository2();
        var result = new GetTypesHandler(allTypesRepository);
        return result;
    }

    private static AllTypesRepository GetAllTypesRepository()
    {
        var allTypesRepository = new AllTypesRepository(_decompileWorkspace);
        return allTypesRepository;
    }

    private static IlSpyTypesInReferencesSearcher GetAllTypesRepository2()
    {
        var assemblyResolverFactory = new AssemblyResolverFactory(_peFileCache);
        var allTypesRepository = new IlSpyTypesInReferencesSearcher(_decompileWorkspace, assemblyResolverFactory);
        return allTypesRepository;
    }

    public static NavigationHandlerBase<DecompiledLocationRequest, LocationsResponse> CreateGetTypeMembersHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var typeMembersFinder = new TypeMembersFinder();
        var ilSpyTypeMembersFinder = new IlSpyTypeMembersFinder(typeMembersFinder, decompilerFactory);
        var ilSpyGetMembersCommandFactory = new ClassLevelGetMembersCommandFactory(
            ilSpySymbolFinder,
            ilSpyTypeMembersFinder,
            decompilerFactory);
        var assemblyLevelGetMembersCommandFactory = new AssemblyLevelGetMembersCommandFactory(
            ilSpySymbolFinder,
            ilSpyTypeMembersFinder,
            decompilerFactory);
        var roslynGetTypeMembersCommandFactory = new RoslynGetTypeMembersCommandFactory(_workspace);
        var result = new NavigationHandlerBase<DecompiledLocationRequest, LocationsResponse>(
            roslynGetTypeMembersCommandFactory,
            ilSpyGetMembersCommandFactory,
            assemblyLevelGetMembersCommandFactory);
        return result;
    }

    public static DecompileAssemblyHandler CreateDecompileAssemblyHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var result = new DecompileAssemblyHandler(decompilerFactory);
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
        var addExternalAssemblyDirectoryHandler = CreateAddExternalAssemblyDirectoryHandler();
        var addMemoryDumpAssembliesHandler = CreateAddMemoryDumpAssembliesHandler();
        var addProcessAssembliesHandler = CreateAddProcessAssembliesHandler();
        var uniqClrStackHandler = CreateUniqCallStackHandler();
        var decompileFrame = DecompileFrameHandler();
        var getAssemblyTypesHandler = GetAssemblyTypesHandler();
        var getAssembliesHandler = GetAssembliesHandler();
        var getSymbolInfoHandler = CreateGetSymbolInfoHandler();
        var decompileAssemblyHandler = CreateDecompileAssemblyHandler();
        var nugetSearcher = new NugetSearcher();

        var findMethodByNameHandler = new FindMethodByNameHandler(
            new AllTypesRepositoryByName(
                _decompileWorkspace,
                new AssemblyResolverFactory(_peFileCache) ),
            new IlSpySymbolFinder(_decompilerTypeSystemFactory),
            new DecompilerFactory(_decompilerTypeSystemFactory),
            _workspace
        );
        
        var handlers = new Dictionary<string, IHandler>
        {
            { Endpoints.DecompileGotoDefinition, gotoDefinitionHandler },
            { Endpoints.DecompileFindImplementations, decompileFindImplementationsHandler },
            { Endpoints.DecompiledSource, decompiledSourceHandler },
            { Endpoints.DecompileFindUsages, decompileFindUsagesHandler },
            { Endpoints.GetAssemblyTypes, getAssemblyTypesHandler },
            { Endpoints.GetTypes, getTypesHandlers },
            { Endpoints.GetTypeMembers, getTypeMembersHandler },
            { Endpoints.AddExternalAssemblyDirectory, addExternalAssemblyDirectoryHandler },
            { Endpoints.AddMemoryDumpAssemblies, addMemoryDumpAssembliesHandler },
            { Endpoints.AddProcessAssemblies, addProcessAssembliesHandler },
            { Endpoints.UniqCallStack, uniqClrStackHandler},
            { Endpoints.DecompileFrame, decompileFrame},
            { Endpoints.GetAssemblies, getAssembliesHandler },
            { Endpoints.SymbolInfo, getSymbolInfoHandler },
            { Endpoints.DecompileAssembly, decompileAssemblyHandler },
            { Endpoints.SearchNuget, new SearchNugetHandler(nugetSearcher) },
            { Endpoints.GetNugetPackageVersions, new GetNugetPackageVersionsHandler() },
            { Endpoints.AddNugetPackageAndDependencies, new AddNugetPackageAndDependenciesHandler(new NugetPackageDownloader(_decompileWorkspace)) },
            { Endpoints.AddNugetPackage, new AddNugetPackageHandler(new NugetPackageDownloader(_decompileWorkspace)) },
            { Endpoints.GetNugetPackageDependencyGroups, new GetNugetPackageDependencyGroupsHandler() },
            { Endpoints.FindMethodByName, findMethodByNameHandler },
            { Endpoints.FindMethodByStackFrame, new FindMethodByStackFrameHandler(
                findMethodByNameHandler) },
            { Endpoints.SearchNugetFromLocation, new SearchNugetForLocationHandler(
                nugetSearcher,
                getSymbolInfoHandler) },
            { Endpoints.SearchMembers, new SearchMembersHandler(
                new MemberSearcher(_decompileWorkspace),
                new IlSpySymbolFinder(_decompilerTypeSystemFactory),
                new DecompilerFactory(_decompilerTypeSystemFactory),
                _workspace ) },
        };

        var router = new Router(handlers);
        
        var result = new Host(_stdIn, _sharedTextWriter, router);
        return result;
    }

    private static GetAssembliesHandler GetAssembliesHandler()
    {
        var getAssembliesCommandFactory = new GetAssembliesCommandFactory(_decompileWorkspace);
        var getAssembliesHandler = new GetAssembliesHandler(getAssembliesCommandFactory);
        return getAssembliesHandler;
    }

    private static GetAssemblyTypesHandler GetAssemblyTypesHandler()
    {
        var allTypesRepository = GetAllTypesRepository();
        var getAssemblyTypesHandler = new GetAssemblyTypesHandler(allTypesRepository);
        return getAssemblyTypesHandler;
    }

    public static AddExternalAssemblyDirectoryHandler CreateAddExternalAssemblyDirectoryHandler()
    {
        var result = new AddExternalAssemblyDirectoryHandler(_decompileWorkspace);
        return result;
    }

    public static AddMemoryDumpAssembliesHandler CreateAddMemoryDumpAssembliesHandler()
    {
        var memoryDumpLoader = GetMemoryDumpLoader();
        var result =  new AddMemoryDumpAssembliesHandler(memoryDumpLoader);
        return result;
    }

    public static UniqCallStackHandler CreateUniqCallStackHandler()
    {
        var result = new UniqCallStackHandler(new UniqCallStackProvider(_dataTargetProvider));
        return result;
    }
    
    public static DecompileFrameHandler DecompileFrameHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var result = new DecompileFrameHandler(new FrameDecompiler(
            _dataTargetProvider,
            decompilerFactory,
            new ClrMdDllExtractor(),
            new MethodNodeInTypeAstFinder()));
        return result;
    }
    
    public static AddProcessAssembliesHandler CreateAddProcessAssembliesHandler()
    {
        var memoryDumpLoader = GetMemoryDumpLoader();
        var result =  new AddProcessAssembliesHandler(memoryDumpLoader);
        return result;
    }
}
