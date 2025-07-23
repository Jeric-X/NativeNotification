using NativeNotification.Interface;

namespace NativeNotification.Common;

public class SessionHistory
{
    private readonly Dictionary<string, Dictionary<string, ActionButton>> _handlerList = [];
    private Dictionary<string, INotificationInternal> _sessionList = [];

    public Action? GetAction(string notificationId, string actionId)
    {
        if (_handlerList.TryGetValue(notificationId, out var buttonList))
        {
            if (buttonList.TryGetValue(actionId, out var button))
            {
                return button.Callback;
            }
        }
        return null;
    }

    public bool IsActionExist(string actionId)
    {
        foreach (var buttonList in _handlerList.Values)
        {
            if (buttonList.ContainsKey(actionId))
            {
                return true;
            }
        }

        return false;
    }

    public void AddSession(string id, INotificationInternal session)
    {
        if (session.Buttons.Count != 0)
        {
            _handlerList.TryAdd(id, []);
            var buttonList = _handlerList[id];
            foreach (var button in session.Buttons)
            {
                buttonList.TryAdd(button.ActionId.ToString(), button);
            }
        }
        _sessionList.TryAdd(id, session);
    }

    public void Reset()
    {
        _handlerList.Clear();
        _sessionList.Clear();
    }

    public void RemoveAll()
    {
        _handlerList.Clear();
        var temp = _sessionList;
        _sessionList = [];

        foreach (var session in temp)
        {
            session.Value.Remove();
        }
    }

    public void Remove(string id)
    {
        _handlerList.Remove(id);
        if (_sessionList.TryGetValue(id, out var session))
        {
            session.SetIsAlive(false);
        }
        _sessionList.Remove(id);
    }
    public INotificationInternal? GetNotification(string id)
    {
        if (_sessionList.TryGetValue(id, out var session))
        {
            return session;
        }
        return null;
    }

    public IEnumerable<INotificationInternal> GetAll()
    {
        return _sessionList.Values;
    }
}