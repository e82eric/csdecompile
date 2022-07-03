public class InSourceGotoDefinitionPropertyCaller
{
    public void Run()
    {
        new InSourceGotoDefinitionPropertyTarget().BasicProperty = "0";
        var t = new InSourceGotoDefinitionPropertyTarget().BasicProperty;
    }
}