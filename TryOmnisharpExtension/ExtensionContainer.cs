using System.Composition;
using Autofac;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

[Export]
[Shared]
public class ExtensionContainer
{
    private readonly IContainer _container;

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

        _container = externalAssembliesContainerBuilder.Build();
    }

    public IContainer Container => _container;
}