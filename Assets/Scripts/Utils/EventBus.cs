using System;
using System.Collections.Generic;

public abstract class Event
{
    public abstract void Initialize(params object[] parameters);
}

public class EventBus : Singleton<EventBus>
{
    public delegate void EventCallback<EventType>(in EventType callback) where EventType : Event;
    private readonly Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

    public void Subscribe<EventType>(EventCallback<EventType> callback) where EventType : Event
    {
        Type eventType = typeof(EventType);
        if (!subscribers.ContainsKey(eventType))
        {
            subscribers.Add(eventType, new List<Delegate>());
        }
        subscribers[eventType].Add(callback);
    }
    public void Unsubscribe<EventType>(EventCallback<EventType> callback) where EventType : Event
    {
        Type eventType = typeof(EventType);
        if (subscribers.TryGetValue(eventType, out List<Delegate> subscriptions))
        {
            subscriptions.Remove(callback);
        }
    }

    public void Raise<EventType>(params object[] parameters) where EventType : Event, new()
    {
        Type eventType = typeof(EventType);
        EventType raisingEvent = new EventType();
        raisingEvent.Initialize(parameters);
        if (subscribers.TryGetValue(eventType, out List<Delegate> subscriptions))
        {
            foreach (Delegate callback in subscriptions)
            {
                ((EventCallback<EventType>)callback)?.Invoke(raisingEvent);
            }
        }
    }

    public void Clear()
    {
        subscribers.Clear();
    }
}
