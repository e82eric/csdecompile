using System.Composition;
using Autofac;
using TryOmnisharpExtension.FindImplementations;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.GetSource;
using TryOmnisharpExtension.GotoDefinition;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

[Export]
[Shared]
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

        // externalAssembliesContainerBuilder.RegisterType<TypeUsedInTypeFinder>();

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
        // externalAssembliesContainerBuilder.RegisterType<IlSpyPropertyImplementationFinder>();
        // externalAssembliesContainerBuilder.RegisterType<IlSpyEventImplementationFinder>();

        externalAssembliesContainerBuilder.RegisterType<AnalyzerScope>();
        
        externalAssembliesContainerBuilder.RegisterType<TypesThatUseTypeAsBaseTypeMetadataScanner>();
        // externalAssembliesContainerBuilder.RegisterType<MethodImplementedByAnalyzer>();
        // externalAssembliesContainerBuilder.RegisterType<PropertyImplementedByAnalyzer>();
        // externalAssembliesContainerBuilder.RegisterType<EventImplementedByAnalyzer>();
        
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

        // externalAssembliesContainerBuilder.RegisterType<MethodUsedByAnalyzer>();
        // externalAssembliesContainerBuilder.RegisterType<MethodInMethodBodyFinder>();

        // externalAssembliesContainerBuilder.RegisterType<PropertyUsedByAnalyzer>();
        // externalAssembliesContainerBuilder.RegisterType<PropertyInMethodBodyFinder>();
                
        // externalAssembliesContainerBuilder.RegisterType<EventUsedByAnalyzer>();
        // externalAssembliesContainerBuilder.RegisterType<EventInMethodBodyFinder>();
        
        // externalAssembliesContainerBuilder.RegisterType<TypeInMethodDefinitionFinder>();
        // externalAssembliesContainerBuilder.RegisterType<TypeInMethodBodyFinder>();

        externalAssembliesContainerBuilder.RegisterType<FieldInTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyFieldFinder>();
        
        // externalAssembliesContainerBuilder.RegisterType<FieldInMethodBodyFinder>();
        // externalAssembliesContainerBuilder.RegisterType<FieldUsedByAnalyzer>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyFieldUsagesFinder>();

        externalAssembliesContainerBuilder.RegisterType<TypeUsedByTypeIlScanner>();
        // externalAssembliesContainerBuilder.RegisterType<TypeUsedInTypeFinder2>();
        
        externalAssembliesContainerBuilder.RegisterType<MethodUsedByMetadataScanner>();
        // externalAssembliesContainerBuilder.RegisterType<MemberUsedByMetadataScanner>();
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