namespace NativeNotification.Interface;

public class ActionButton
{
    public string Text { get; set; }
    public string ActionId { get; }
    public Action? Callback { get; set; }

    public ActionButton(string text)
    {
        Text = text;
        ActionId = Guid.NewGuid().ToString();
    }

    public ActionButton(string text, Action callback)
    {
        Text = text;
        Callback = callback;
        ActionId = Guid.NewGuid().ToString();
    }

    public ActionButton(string text, Action callback, string actionId)
    {
        Text = text;
        Callback = callback;
        ActionId = actionId;
    }

    public ActionButton(string text, string actionId)
    {
        Text = text;
        ActionId = actionId;
    }
}