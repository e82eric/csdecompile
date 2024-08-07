namespace CsDecompileLib
{
    public static class Endpoints
    {
        public const string DecompileGotoDefinition = "/gotodefinition";
        public const string DecompileFindImplementations = "/findimplementations";
        public const string DecompiledSource = "/decompiledsource";
        public const string SymbolInfo = "/symbolinfo";
        public const string DecompileFindUsages = "/findusages";
        public const string GetTypes = "/gettypes";
        public const string GetTypeMembers = "/gettypemembers";
        public const string AddExternalAssemblyDirectory = "/addexternalassemblydirectory";
        public const string AddMemoryDumpAssemblies = "/addmemorydumpassemblies";
        public const string AddProcessAssemblies = "/addprocessassemblies";
        public const string UniqCallStack = "/uniqCallStacks";
        public const string DecompileFrame = "/decompileframe";
        public const string GetAssemblies = "/getassemblies";
        public const string GetAssemblyTypes = "/getassemblytypes";
        public const string DecompileAssembly = "/decompileassembly";
        public const string SearchNuget = "/searchnuget";
        public const string SearchNugetFromLocation = "/searchnugetfromlocation";
        public const string GetNugetPackageVersions = "/getnugetpackageversions";
        public const string AddNugetPackageAndDependencies = "/addnugetpackageanddependencies";
        public const string AddNugetPackage = "/addnugetpackage";
        public const string GetNugetPackageDependencyGroups = "/getnugetpackagedependencygroups";
        public const string FindMethodByName = "/findmethodbyname";
        public const string FindMethodByStackFrame = "/findmethodbystackframe";
        public const string SearchMembers = "/searchmembers";
    }
}