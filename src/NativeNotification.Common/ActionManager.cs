using NativeNotification.Interface;

namespace NativeNotification.Common;

public class ActionManager<NotificationIdType> where NotificationIdType : notnull
{
    private readonly Dictionary<NotificationIdType, Dictionary<string, ActionButton>> _handlerList = [];
    private readonly Dictionary<NotificationIdType, INotification> _sessionList = [];

    public void OnActivated(NotificationIdType id, string buttonId)
    {
        var found = _handlerList.TryGetValue(id, out Dictionary<string, ActionButton>? buttonList);
        if (found)
        {
            buttonList!.TryGetValue(buttonId, out ActionButton? button);
            button?.Invoke();
            _handlerList.Remove(id);
        }

        _sessionList.Remove(id);
    }

    public void OnClosed(NotificationIdType id)
    {
        _handlerList.Remove(id);
        _sessionList.Remove(id);
    }

    public void AddButton(NotificationIdType id, ActionButton button, INotification session)
    {
        _sessionList.TryAdd(id, session);
        _handlerList.TryAdd(id, []);

        var buttonList = _handlerList[id];
        buttonList.TryAdd(button.Uid.ToString(), button);
    }

    public void AddButtons(NotificationIdType id, IEnumerable<ActionButton> buttons, INotification session)
    {
        foreach (var item in buttons)
        {
            AddButton(id, item, session);
        }
    }

    public void Clear()
    {
        _handlerList.Clear();
        _handlerList.Clear();
    }
}