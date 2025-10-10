using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class InputService : IDisposable
{
    private readonly InputActionAsset _asset;
    private readonly InputSubscriberContainer _inputSubscribers;

    public InputService(InputActionAsset asset)
    {
        _asset = asset ?? throw new ArgumentNullException(nameof(asset));
        _inputSubscribers = new InputSubscriberContainer();
    }

    public void Subscribe(ActionPath path, Action<InputAction.CallbackContext> action, InputActionType type = InputActionType.Performed)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        var actionMap = _asset.FindActionMap(path.Map) ?? throw new ArgumentException($"Action map '{path.Map}' not found.");
        var inputAction = actionMap.FindAction(path.Action) ?? throw new ArgumentException($"Action '{path.Action}' not found in map '{path.Map}'.");

        var subscriber = new InputSubscriber(inputAction, action, type);
        _inputSubscribers.Add(path, subscriber);
        subscriber.Enable();
    }

    public void Subscribe(ActionPath path, params (Action<InputAction.CallbackContext> action, InputActionType type)[] actions)
    {
        if (actions == null || actions.Length == 0)
            throw new ArgumentNullException(nameof(actions));

        var actionMap = _asset.FindActionMap(path.Map) ?? throw new ArgumentException($"Action map '{path.Map}' not found.");
        var inputAction = actionMap.FindAction(path.Action) ?? throw new ArgumentException($"Action '{path.Action}' not found in map '{path.Map}'.");

        foreach (var (action, type) in actions)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var subscriber = new InputSubscriber(inputAction, action, type);
            _inputSubscribers.Add(path, subscriber);
            subscriber.Enable();
        }
    }

    public void Unsubscribe(ActionPath path)
    {
        _inputSubscribers.Remove(path);
    }

    public void Unsubscribe(ActionPath path, InputActionType type)
    {
        _inputSubscribers.Remove(path, type);
    }

    public void DisableAll()
    {
        _asset.Disable();
    }

    public void EnableAll()
    {
        _asset.Enable();
    }

    public void DisableMap(string map)
    {
        if (string.IsNullOrEmpty(map)) throw new ArgumentNullException(nameof(map));

        var actionMap = _asset.FindActionMap(map) ?? throw new ArgumentException($"Action map '{map}' not found.");
        actionMap.Disable();
    }

    public void EnableMap(string map)
    {
        if (string.IsNullOrEmpty(map)) throw new ArgumentNullException(nameof(map));

        var actionMap = _asset.FindActionMap(map) ?? throw new ArgumentException($"Action map '{map}' not found.");
        actionMap.Enable();
    }

    public void DisableAction(string map, string action)
    {
        if (string.IsNullOrEmpty(map)) throw new ArgumentNullException(nameof(map));
        if (string.IsNullOrEmpty(action)) throw new ArgumentNullException(nameof(action));

        var actionMap = _asset.FindActionMap(map) ?? throw new ArgumentException($"Action map '{map}' not found.");
        var inputAction = actionMap.FindAction(action) ?? throw new ArgumentException($"Action '{action}' not found in map '{map}'.");

        inputAction.Disable();
    }

    public void EnableAction(string map, string action)
    {
        if (string.IsNullOrEmpty(map)) throw new ArgumentNullException(nameof(map));
        if (string.IsNullOrEmpty(action)) throw new ArgumentNullException(nameof(action));

        var actionMap = _asset.FindActionMap(map) ?? throw new ArgumentException($"Action map '{map}' not found.");
        var inputAction = actionMap.FindAction(action) ?? throw new ArgumentException($"Action '{action}' not found in map '{map}'.");

        inputAction.Enable();
    }

    public void Dispose()
    {
        _inputSubscribers.Dispose();
    }
}

public class InputSubscriberContainer : IDisposable
{
    private Dictionary<(ActionPath Path, InputActionType Type), InputSubscriber> _subscribers
        = new Dictionary<(ActionPath, InputActionType), InputSubscriber>();

    public void Add(ActionPath path, InputSubscriber subscriber)
    {
        if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
        var key = (path, subscriber.Type);

        if (_subscribers.ContainsKey(key))
        {
            throw new InvalidOperationException($"Subscriber for {path} and {subscriber.Type} already exists.");
        }

        _subscribers[key] = subscriber;
    }

    public void Remove(ActionPath path)
    {
        var keysToRemove = _subscribers.Keys.Where(k => k.Path == path).ToList();
        foreach (var key in keysToRemove)
        {
            _subscribers[key].Dispose();
            _subscribers.Remove(key);
        }
    }

    public void Remove(ActionPath path, InputActionType type)
    {
        var key = (path, type);
        if (_subscribers.TryGetValue(key, out var subscriber))
        {
            subscriber.Dispose();
            _subscribers.Remove(key);
        }
    }

    public IEnumerable<InputSubscriber> GetAllSubscribers()
    {
        return _subscribers.Values;
    }

    public IEnumerable<KeyValuePair<ActionPath, InputSubscriber>> GetAllSubscribersWithPath()  // Упрощённо, без типов
    {
        return _subscribers.Select(kvp => KeyValuePair.Create(kvp.Key.Path, kvp.Value));
    }

    public InputSubscriber FindSubscriber(ActionPath path, InputActionType? type = null)
    {
        if (type.HasValue)
        {
            var key = (path, type.Value);
            return _subscribers.GetValueOrDefault(key);
        }
        else
        {
            return _subscribers.FirstOrDefault(kvp => kvp.Key.Path == path).Value;
        }
    }

    public void Dispose()
    {
        foreach (var subscriber in _subscribers.Values)
        {
            subscriber.Dispose();
        }
        _subscribers.Clear();
    }
}

public enum InputActionType { Started, Performed, Canceled }

public sealed class InputSubscriber : IDisposable
{
    public InputAction Action { get; }
    public Action<InputAction.CallbackContext> Callback { get; }
    public InputActionType Type { get; }

    private bool disposed = false;

    public InputSubscriber(InputAction action, Action<InputAction.CallbackContext> callback, InputActionType type)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        Type = type;

        switch (Type)
        {
            case InputActionType.Started:
                Action.started += Callback;
                break;
            case InputActionType.Performed:
                Action.performed += Callback;
                break;
            case InputActionType.Canceled:
                Action.canceled += Callback;
                break;
            default:
                throw new ArgumentException("Invalid InputActionType", nameof(type));
        }
    }

    public void Enable()
    {
        if (disposed) throw new ObjectDisposedException(nameof(InputSubscriber));
        Action?.Enable();
    }

    public void Disable()
    {
        if (disposed) throw new ObjectDisposedException(nameof(InputSubscriber));
        Action?.Disable();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            switch (Type)
            {
                case InputActionType.Started:
                    Action.started -= Callback;
                    break;
                case InputActionType.Performed:
                    Action.performed -= Callback;
                    break;
                case InputActionType.Canceled:
                    Action.canceled -= Callback;
                    break;
            }
        }
        disposed = true;
    }

    ~InputSubscriber()
    {
        Dispose(false);
    }
}

public readonly struct ActionPath
{
    public readonly string Map;
    public readonly string Action;

    public ActionPath(string map, string action)
    {
        Map = map;
        Action = action;
    }

    public override string ToString()
    {
        return $"{Map}/{Action}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Map, Action);
    }

    public override bool Equals(object obj)
    {
        return obj is ActionPath other &&
               Map == other.Map &&
               Action == other.Action;
    }

    public static bool operator ==(ActionPath left, ActionPath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ActionPath left, ActionPath right)
    {
        return !(left == right);
    }
}
