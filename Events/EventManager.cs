using System.Collections.Generic;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// The Event Manager (Publisher). Stores a list of subscribers by event types.
/// Allows subscribing, unsubscribing, and notifying listeners dynamically.
/// </summary>
public class EventManager
{
    private readonly Dictionary<Type, List<object>> _listeners = new();
    public void Subscribe<TEvent>(IEventListener<TEvent> listener)
    {
        var eventType = typeof(TEvent);

        if (!_listeners.ContainsKey(eventType))
        {
            _listeners[eventType] = new List<object>();
        }

        if (!_listeners[eventType].Contains(listener))
        {
            _listeners[eventType].Add(listener);
        }
    }

    public void Unsubscribe<TEvent>(IEventListener<TEvent> listener)
    {
        var eventType = typeof(TEvent);

        if (_listeners.ContainsKey(eventType))
        {
            _listeners[eventType].Remove(listener);
        }
    }

    public void Notify<TEvent>(TEvent eventData)
    {
        var eventType = typeof(TEvent);

        if (_listeners.ContainsKey(eventType))
        {
            var currentListeners = _listeners[eventType].ToList();
            foreach (var listener in currentListeners)
            {
                ((IEventListener<TEvent>)listener).OnEvent(eventData);
            }
        }
    }
}