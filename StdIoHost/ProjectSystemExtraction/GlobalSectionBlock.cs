using System.Collections.Immutable;

internal sealed class GlobalSectionBlock : SectionBlock
{
    private GlobalSectionBlock(string name, ImmutableArray<Property> properties)
        : base(name, properties)
    {
    }

    public static GlobalSectionBlock Parse(string headerLine, Scanner scanner)
    {
        var (name, properties) = ParseNameAndProperties(
            "GlobalSection", "EndGlobalSection", headerLine, scanner);

        return new GlobalSectionBlock(name, properties);
    }
}