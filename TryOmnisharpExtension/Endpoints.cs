namespace TryOmnisharpExtension
{
    internal static class Endpoints
    {
        public const string DecompileGotoDefinition = "/decompilegotodefinition";
        public const string DecompileFindImplementations = "/decompilefindimplementations";
        public const string DecompiledSource = "/decompiledsource";
        public const string DecompileFindUsages = "/decompilefindusages";
        public const string GetTypes = "/gettypes";
        public const string GetTypeMembers = "/gettypemembers";
        public const string AddExternalAssemblyDirectory = "/addexternalassemblydirectory";
        public const string SearchExternalAssembliesForType = "/searchexternalassembliesfortype";
        public const string LoadAssemblies = "/loadassemblies";
    }
}