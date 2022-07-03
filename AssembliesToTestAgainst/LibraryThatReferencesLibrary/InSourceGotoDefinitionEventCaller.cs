using System;

public class InSourceGotoDefinitionEventCaller
{
    public void Run()
    {
        var obj = new InSourceGotoDefinitionEventTarget();
        obj.BasicEvent += OnBasicEvent;
        obj.BasicEvent -= OnBasicEvent;
    }

    private void OnBasicEvent(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}