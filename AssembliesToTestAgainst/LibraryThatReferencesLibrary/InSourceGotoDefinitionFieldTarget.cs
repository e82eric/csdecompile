public class InSourceGotoDefinitionFieldTarget
{
    private string _basicField;

    public InSourceGotoDefinitionFieldTarget()
    {
        _basicField = "0";
    }

    public void Run()
    {
        var a = _basicField;
    }
}