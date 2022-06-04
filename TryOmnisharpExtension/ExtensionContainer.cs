using System.Composition;
using Autofac;
using IlSpy.Analyzer.Extraction;
using TryOmnisharp.Decompiler.IlSpy2;
using TryOmnisharpExtension.FindUsages;
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

        externalAssembliesContainerBuilder.RegisterType<TypeUsedInTypeFinder>();

        externalAssembliesContainerBuilder.RegisterType<IlSpySymbolFinder>();

        externalAssembliesContainerBuilder.RegisterType<IlSpyDecompiledSourceCommandFactory>();

        externalAssembliesContainerBuilder.RegisterType<TypenFinder2>();
        externalAssembliesContainerBuilder.RegisterType<MethodInTypeFinder2>();
        externalAssembliesContainerBuilder.RegisterType<PropertyInTypeFinder2>();
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

        externalAssembliesContainerBuilder.RegisterType<IlSpyBaseTypeUsageFinder2>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyMethodImplementationFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyPropertyImplementationFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyEventImplementationFinder>();

        externalAssembliesContainerBuilder.RegisterType<AnalyzerScope>();
        
        externalAssembliesContainerBuilder.RegisterType<TypeUsedByAnalyzer2>();
        externalAssembliesContainerBuilder.RegisterType<MethodImplementedByAnalyzer>();
        externalAssembliesContainerBuilder.RegisterType<PropertyImplementedByAnalyzer>();
        externalAssembliesContainerBuilder.RegisterType<EventImplementedByAnalyzer>();
        
        externalAssembliesContainerBuilder.RegisterType<ExternalAssembliesDecompileWorkspace>()
            .As<IDecompileWorkspace>();
        
        externalAssembliesContainerBuilder
            .RegisterType<IlSpyExternalAssembliesCommandFactory<FindUsagesResponse>>();


        externalAssembliesContainerBuilder.RegisterType<ExternalAssembliesFindUsagesCommandFactory>()
            .As<IDecompilerCommandFactory<INavigationCommand<FindUsagesResponse>>>();
        
        externalAssembliesContainerBuilder.RegisterType<IlSpyUsagesFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyMethodUsagesFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyPropertyUsagesFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyEventUsagesFinder>();

        externalAssembliesContainerBuilder.RegisterType<MethodUsedByAnalyzer>();
        externalAssembliesContainerBuilder.RegisterType<MethodInMethodBodyFinder>();

        externalAssembliesContainerBuilder.RegisterType<PropertyUsedByAnalyzer>();
        externalAssembliesContainerBuilder.RegisterType<PropertyInMethodBodyFinder>();
                
        externalAssembliesContainerBuilder.RegisterType<EventUsedByAnalyzer>();
        externalAssembliesContainerBuilder.RegisterType<EventInMethodBodyFinder>();
        
        externalAssembliesContainerBuilder.RegisterType<TypeInMethodDefinitionFinder>();
        externalAssembliesContainerBuilder.RegisterType<TypeInMethodBodyFinder>();

        externalAssembliesContainerBuilder.RegisterType<FieldInTypeFinder>();
        externalAssembliesContainerBuilder.RegisterType<IlSpyFieldFinder>();
        Container = externalAssembliesContainerBuilder.Build();
    }

    public IContainer Container { get; }
}