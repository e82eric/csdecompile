using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Extensions.Logging;
using StdIoHost.ProjectSystemExtraction;
using StdIoHost.SimpleProjectSystem;
using CsDecompileLib;
using CsDecompileLib.ExternalAssemblies;
using CsDecompileLib.FindImplementations;
using CsDecompileLib.FindUsages;
using CsDecompileLib.GetMembers;
using CsDecompileLib.GetSource;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;

namespace StdIoHost;

internal static class OmniSharpApplication
{
    private static IOmniSharpWorkspace _workspace;
    // private static ILoggerFactory _loggerFactory;
    private static PeFileCache _peFileCache;
    private static IlSpyTypeSystemFactory _decompilerTypeSystemFactory;
    private static IDecompileWorkspace _decompileWorkspace;
    private static TextReader _stdIn;
    private static SharedTextWriter _sharedTextWriter;

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
        _decompileWorkspace = new NoSolutionDecompileWorkspace(_peFileCache);
        return Task.FromResult(0);
    }

    private static IOmniSharpWorkspace GetWorkspace(StdioEventEmitter eventEmitter)
    {
        var result = new SimpleDecompileWorkspace(eventEmitter);
        return result;
    }

    public static GetSymbolInfoHandler CreateGetSymbolInfoHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var getSymbolInfoCommandFactory = new GetSymbolInfoCommandFactory();
        var roslynLocationToCommandFactory = new RoslynLocationToCommandFactory<INavigationCommand<SymbolInfo>>(
            _workspace,
            ilSpySymbolFinder,
            getSymbolInfoCommandFactory);
        var assemblyLevelVariableCommandProvider =
            new ClassLevelVariableCommandProvider<INavigationCommand<SymbolInfo>>(
                ilSpySymbolFinder,
                getSymbolInfoCommandFactory,
                decompilerFactory);
        var ilSpySymbolInfoFinder = new IlSpyCommandFactory<INavigationCommand<SymbolInfo>>(
            assemblyLevelVariableCommandProvider,
            getSymbolInfoCommandFactory);
        var result = new GetSymbolInfoHandler(roslynLocationToCommandFactory, ilSpySymbolInfoFinder);
        return result;
    }

    public static DecompileGotoDefinitionHandler CreateGoToDefinitionHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var typeInTypeFinder = new TypeInTypeFinder();
        var ilSpyTypeFinder = new IlSpyDefinitionFinderBase<ITypeDefinition>(typeInTypeFinder, typeInTypeFinder, decompilerFactory);
        var methodInTypeFinder = new MethodInTypeFinder();
        var ilSpyMemberFinder = new IlSpyDefinitionFinderBase<IMethod>(methodInTypeFinder, typeInTypeFinder, decompilerFactory);
        var propertyInTypeFinder = new PropertyInTypeFinder();
        var ilSpyPropertyFinder = new IlSpyDefinitionFinderBase<IProperty>(propertyInTypeFinder, typeInTypeFinder, decompilerFactory);
        var eventInTypeFinder = new EventInTypeFinder();
        var ilSpyEventFinder = new IlSpyDefinitionFinderBase<IEvent>(eventInTypeFinder, typeInTypeFinder, decompilerFactory);
        var fieldInTypeFinder = new FieldInTypeFinder();
        var ilSpyFieldFinder = new IlSpyDefinitionFinderBase<IField>(fieldInTypeFinder, typeInTypeFinder, decompilerFactory);
        var gotoDefinitionCommandFactory = new GotoDefinitionCommandFactory(
            ilSpyTypeFinder, ilSpyMemberFinder, ilSpyPropertyFinder, ilSpyEventFinder, ilSpyFieldFinder);
        var roslynSymbolInfoFinder = new RoslynLocationToCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>>(
            _workspace,
            ilSpySymbolFinder,
            gotoDefinitionCommandFactory);
        var memberInTypeFinder = new MemberInTypeFinder();
        var assemblyLevelVariableCommandProvider = new AssemblyLevelVariableCommandProvider<INavigationCommand<DecompileGotoDefinitionResponse>>(
            decompilerFactory,
            memberInTypeFinder,
            ilSpySymbolFinder,
            gotoDefinitionCommandFactory);
        var classLevelVariableCommandProvider = new ClassLevelVariableCommandProvider<INavigationCommand<DecompileGotoDefinitionResponse>>(
            ilSpySymbolFinder,
            gotoDefinitionCommandFactory,
            decompilerFactory);
        var classLevelCommandFactory = new IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>>(
            classLevelVariableCommandProvider,
            gotoDefinitionCommandFactory);
        var assemblyLevelCommandFactory = new IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>>(
            assemblyLevelVariableCommandProvider,
            gotoDefinitionCommandFactory);
        var result = new DecompileGotoDefinitionHandler(
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

    public static DecompileFindImplementationsHandler CreateFindImplementationsHandler()
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
        var everywhereSymbolInfoFinder2 = new EverywhereSymbolInfoFinder<FindImplementationsResponse>(
            _workspace, ilSpySymbolFinder, ilSpyFindImplementationsCommandFactoryTemp);
        var memberInTypeFinder = new MemberInTypeFinder();
        var assemblyLevelVariableCommandProvider = new AssemblyLevelVariableCommandProvider<INavigationCommand<FindImplementationsResponse>>(
            decompilerFactory,
            memberInTypeFinder,
            ilSpySymbolFinder,
            ilSpyFindImplementationsCommandFactoryTemp);
        var classLevelVariableCommandProvider = new ClassLevelVariableCommandProvider<INavigationCommand<FindImplementationsResponse>>(
            ilSpySymbolFinder,
            ilSpyFindImplementationsCommandFactoryTemp,
            decompilerFactory);
        var everyWhereFindImplementationsCommandFactory = new EveryWhereFindImplementationsCommandFactory<FindImplementationsResponse>(
            ilSpyFindImplementationsCommandFactoryTemp,
            _decompileWorkspace);
        var classLevelFindImplementationsCommandFactory = new GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse>(
            classLevelVariableCommandProvider,
            everyWhereFindImplementationsCommandFactory);
        var assemblyLevelFindImplementationsCommandFactory = new GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse>(
            assemblyLevelVariableCommandProvider,
            everyWhereFindImplementationsCommandFactory);
        var result = new DecompileFindImplementationsHandler(
            everywhereSymbolInfoFinder2,
            classLevelFindImplementationsCommandFactory,
            assemblyLevelFindImplementationsCommandFactory);
        return result;
    }
    public static DecompileFindUsagesHandler CreateFindUsagesHandler()
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
        var everywhereSymbolInfoFinder2 = new EverywhereSymbolInfoFinder<FindImplementationsResponse>(
            _workspace,
            ilSpySymbolFinder,
            findUsagesCommandFactory);
        var classLevelVariableCommandProvider = new ClassLevelVariableCommandProvider<INavigationCommand<FindImplementationsResponse>>(
            ilSpySymbolFinder,
            findUsagesCommandFactory,
            decompilerFactory);
        var everyWhereFindImplementationsCommandFactory = new EveryWhereFindImplementationsCommandFactory<FindImplementationsResponse>(
            findUsagesCommandFactory,
            _decompileWorkspace);
        var classLevelFindImplementationsCommandFactory = new GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse>(
            classLevelVariableCommandProvider,
            everyWhereFindImplementationsCommandFactory);
        var memberInTypeFinder = new MemberInTypeFinder();
        var assemblyLevelVariableCommandProvider = new AssemblyLevelVariableCommandProvider<INavigationCommand<FindImplementationsResponse>>(
            decompilerFactory,
            memberInTypeFinder,
            ilSpySymbolFinder,
            findUsagesCommandFactory);
        var assemblyLevelFindImplementationsCommandFactory = new GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse>(
            assemblyLevelVariableCommandProvider,
            everyWhereFindImplementationsCommandFactory);
        var result = new DecompileFindUsagesHandler(
            everywhereSymbolInfoFinder2,
            classLevelFindImplementationsCommandFactory,
            assemblyLevelFindImplementationsCommandFactory);
        return result;
    }

    public static GetTypesHandler CreateGetTypesHandlers()
    {
        var allTypesRepository = GetAllTypesRepository();
        var result = new GetTypesHandler(allTypesRepository);
        return result;
    }

    private static AllTypesRepository GetAllTypesRepository()
    {
        var assemblyResolverFactory = new AssemblyResolverFactory(_peFileCache);
        var allTypesRepository = new AllTypesRepository(_decompileWorkspace, assemblyResolverFactory);
        return allTypesRepository;
    }

    public static GetTypeMembersHandler CreateGetTypeMembersHandler()
    {
        var decompilerFactory = new DecompilerFactory(_decompilerTypeSystemFactory);
        var ilSpySymbolFinder = new IlSpySymbolFinder(_decompilerTypeSystemFactory);
        var typeMembersFinder = new TypeMembersFinder();
        var ilSpyTypeMembersFinder = new IlSpyTypeMembersFinder(typeMembersFinder, decompilerFactory);
        var ilSpyGetMembersCommandFactory = new IlSpyGetMembersCommandFactory(ilSpySymbolFinder, ilSpyTypeMembersFinder);
        var roslynGetTypeMembersCommandFactory = new RoslynGetTypeMembersCommandFactory(_workspace);
        var result = new GetTypeMembersHandler(ilSpyGetMembersCommandFactory, roslynGetTypeMembersCommandFactory);
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
        var getAssemblyTypesHandler = GetAssemblyTypesHandler();
        var getAssembliesHandler = GetAssembliesHandler();
        var getSymbolInfoHandler = CreateGetSymbolInfoHandler();
        var decompileAssemblyHandler = CreateDecompileAssemblyHandler();
        
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
            { Endpoints.GetAssemblies, getAssembliesHandler },
            { Endpoints.SymbolInfo, getSymbolInfoHandler },
            { Endpoints.DecompileAssembly, decompileAssemblyHandler },
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
}
