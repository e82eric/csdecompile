using System.Collections.Immutable;

public interface IMSBuildLocator
{
    void RegisterInstance(MSBuildInstance instance);
    ImmutableArray<MSBuildInstance> GetInstances();
}