using System.Collections.Immutable;
using System.Reflection;

public interface IHostServicesProvider
{
    ImmutableArray<Assembly> Assemblies { get; }
}