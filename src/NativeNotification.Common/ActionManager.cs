using NativeNotification.Interface;

namespace NativeNotification.Common;

public class ActionManager<NotificationIdType> where NotificationIdType : notnull
{
    private Dictionary<NotificationIdType, Dictionary<string, ActionButton>> _handlerList = [];
    private Dictionary<NotificationIdType, INotificationInternal> _sessionList = [];

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
        if (_sessionList.TryGetValue(id, out var session))
        {
            session.SetIsAlive(false);
        }
    }

    private void AddButton(NotificationIdType id, ActionButton button, INotificationInternal session)
    {
        _handlerList.TryAdd(id, []);

        var buttonList = _handlerList[id];
        buttonList.TryAdd(button.Uid.ToString(), button);
    }

    private void AddButtons(NotificationIdType id, IEnumerable<ActionButton> buttons, INotificationInternal session)
    {
        foreach (var item in buttons)
        {
            AddButton(id, item, session);
        }
    }

    public void AddSession(NotificationIdType id, INotificationInternal session)
    {
        AddButtons(id, session.Buttons, session);
        _sessionList.TryAdd(id, session);
    }

    public void Reset()
    {
        _handlerList = [];
        _sessionList = [];
    }

    public void RemoveAll()
    {
        _handlerList = [];
        var temp = _sessionList;
        _sessionList = [];

        foreach (var session in temp)
        {
            session.Value.Remove();
        }
    }
}