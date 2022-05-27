namespace TryOmnisharpExtension
{
    public class DecompileInfo : ResponseLocation
    {
        public string AssemblyName { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public string ContainingTypeFullName { get; set; }
        public string AssemblyFilePath { get; set; }
        public string UsageType { get; set; }
        public string NamespaceName { get; set; }
        public string TypeName { get; set; }
        public string BaseTypeName { get; set; }
        public string MethodName { get; set; }
        public string DotNetVersion { get; set; }
        public string AssemblyVersion{ get; set; }
        public string TypeFullName{ get; set; }
        
        public override ResponseLocationType Type => ResponseLocationType.Decompiled;
    }
}