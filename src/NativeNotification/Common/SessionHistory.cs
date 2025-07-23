using NativeNotification.Interface;

namespace NativeNotification.Common;

public class SessionHistory<IdType> where IdType : notnull
{
    private readonly Dictionary<IdType, Dictionary<string, ActionButton>> _handlerList = [];
    private Dictionary<IdType, INotificationInternal<IdType>> _sessionList = [];

    public Action? GetAction(IdType id, string actionId)
    {
        if (_handlerList.TryGetValue(id, out var buttonList))
        {
            if (buttonList.TryGetValue(actionId, out var button))
            {
                return button.Callback;
            }
        }
        return null;
    }

    public void OnClosed(IdType id)
    {
        _handlerList.Remove(id);
        if (_sessionList.TryGetValue(id, out var session))
        {
            session.SetIsAlive(false);
        }
        _sessionList.Remove(id);
    }

    public void AddSession(IdType id, INotificationInternal<IdType> session)
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

    public void Remove(IdType id)
    {
        _handlerList.Remove(id);
        if (_sessionList.TryGetValue(id, out var session))
        {
            session.SetIsAlive(false);
        }
        _sessionList.Remove(id);
    }
    public INotificationInternal<IdType>? Get(IdType id)
    {
        if (_sessionList.TryGetValue(id, out var session))
        {
            return session;
        }
        return null;
    }

    public IEnumerable<INotificationInternal<IdType>> GetAll()
    {
        return _sessionList.Values;
    }
}