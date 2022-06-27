namespace TryOmnisharpExtension
{
    public static class Endpoints
    {
        public const string DecompileGotoDefinition = "/gotodefinition";
        public const string DecompileFindImplementations = "/findimplementations";
        public const string DecompiledSource = "/decompiledsource";
        public const string DecompileFindUsages = "/findusages";
        public const string GetTypes = "/gettypes";
        public const string GetTypeMembers = "/gettypemembers";
        public const string AddExternalAssemblyDirectory = "/addexternalassemblydirectory";
        public const string SearchExternalAssembliesForType = "/searchexternalassembliesfortype";
        public const string LoadAssemblies = "/loadassemblies";
    }
}