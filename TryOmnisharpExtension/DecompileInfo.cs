namespace TryOmnisharpExtension
{
    public class DecompileInfo : ResponseLocation
    {
        public string AssemblyName { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public string ContainingTypeFullName { get; set; }
        public string AssemblyFilePath { get; set; }
        
        public override ResponseLocationType Type => ResponseLocationType.Decompiled;
    }
}