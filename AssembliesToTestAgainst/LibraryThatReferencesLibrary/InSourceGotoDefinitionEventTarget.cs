using System;

public class InSourceGotoDefinitionEventTarget
{
    public event EventHandler<EventArgs> BasicEvent;

    public void Run()
    {
        BasicEvent(this, new EventArgs());
    }
}