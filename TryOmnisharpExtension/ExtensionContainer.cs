using Autofac;
using TryOmnisharpExtension.FindImplementations;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.GetSource;
using TryOmnisharpExtension.GotoDefinition;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

public class ExtensionContainer
{
    public ExtensionContainer()
    {
        var externalAssembliesContainerBuilder = new ContainerBuilder();

        externalAssembliesContainerBuilder.RegisterType<PeFileCache>()
            .SingleInstance();

        externalAssembliesContainerBuilder.RegisterType<AssemblyResolverFactory>();

        externalAssembliesContainerBuilder.RegisterType<ExternalAssemblyTypeSystemFactory>()
            .As<IDecompilerTypeSystemFactory>();

        externalAssembliesContainerBuilder.RegisterType<DecompilerFactory>();

        externalAssembliesContainerBuilder.RegisterType<IlSpySymbolFinder>();

        externalAssembliesContainerBuilder.RegisterType<IlSpyDecompiledSourceCommandFactory>();

        externalAssembliesContainerBuilder.RegisterType<TypeInTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<MethodInTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<PropertyInTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<EventInTypeFinder>();

        externalAssembliesContainerBuilder.RegisterType<IlSpyTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyMemberFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyPropertyFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyEventFinder>();

        externalAssembliesContainerBuilder.RegisterType<GotoDefinitionCommandFactory>()
            .As<ICommandFactory<IGotoDefinitionCommand>>();

        externalAssembliesContainerBuilder.RegisterType<IlSpyCommandFactory<IGotoDefinitionCommand>>();

        externalAssembliesContainerBuilder.RegisterType<IlSpyFindImplementationsCommandFactory>()
            .As<IDecompilerCommandFactory<INavigationCommand<FindImplementationsResponse>>>();
        
        externalAssembliesContainerBuilder
            .RegisterType<IlSpyExternalAssembliesCommandFactory<FindImplementationsResponse>>();

        externalAssembliesContainerBuilder.RegisterType<IlSpyBaseTypeUsageFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyMemberImplementationFinder>();

        externalAssembliesContainerBuilder.RegisterType<AnalyzerScope>();
        
        externalAssembliesContainerBuilder.RegisterType<TypesThatUseTypeAsBaseTypeMetadataScanner>();
        
        externalAssembliesContainerBuilder.RegisterType<ExternalAssembliesDecompileWorkspace>()
            .As<IDecompileWorkspace>();
        
        externalAssembliesContainerBuilder
            .RegisterType<IlSpyExternalAssembliesCommandFactory<FindUsagesResponse>>();


        externalAssembliesContainerBuilder.RegisterType<ExternalAssembliesFindUsagesCommandFactory>()
            .As<IDecompilerCommandFactory<INavigationCommand<FindUsagesResponse>>>();
        
        externalAssembliesContainerBuilder.RegisterType<IlSpyTypeUsagesFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyMethodUsagesFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyPropertyUsagesFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyEventUsagesFinder>();

        externalAssembliesContainerBuilder.RegisterType<FieldInTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyFieldFinder>();
        
        externalAssembliesContainerBuilder.RegisterType<IlSpyFieldUsagesFinder>();

        externalAssembliesContainerBuilder.RegisterType<TypeUsedByTypeIlScanner>();
        
        externalAssembliesContainerBuilder.RegisterType<MethodUsedByMetadataScanner>();
        externalAssembliesContainerBuilder.RegisterType<TypeUsedInTypeFinder3>();
        externalAssembliesContainerBuilder.RegisterType<MemberUsedInTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<EventUsedByMetadataScanner>();
        externalAssembliesContainerBuilder.RegisterType<FieldUsedByMetadataScanner>();
        externalAssembliesContainerBuilder.RegisterType<PropertyUsedByMetadataScanner>();
        
        externalAssembliesContainerBuilder.RegisterType<TypeUsedAsBaseTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<TypesThatUseMemberAsBaseTypeMetadataScanner>();
        externalAssembliesContainerBuilder.RegisterType<MemberOverrideInTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyVariableUsagesFinder>();
        externalAssembliesContainerBuilder.RegisterType<VariableInMethodBodyFinder>();
        
        Container = externalAssembliesContainerBuilder.Build();
    }

    public IContainer Container { get; }
}