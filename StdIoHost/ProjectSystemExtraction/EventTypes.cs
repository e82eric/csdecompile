public static class EventTypes
{
    public const string Error = nameof(Error);
    public const string PackageRestoreStarted = nameof(PackageRestoreStarted);
    public const string PackageRestoreFinished = nameof(PackageRestoreFinished);
    public const string UnresolvedDependencies = nameof(UnresolvedDependencies);
    public const string ProjectConfiguration = nameof(ProjectConfiguration);
}