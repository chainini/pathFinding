

using System;
using System.Collections.Generic;


public static class EventName
{
    public const string GameModeChange = "GameModeChange";
}

public class EventManager
{
    static Dictionary<string,Delegate> events = new Dictionary<string, Delegate>();
    
    public static void On(string eventName, Action handle)
    {
        if (events.TryGetValue(eventName, out var existing))
        {
            var typed = (Action)existing;
            typed += handle;
            events[eventName] = typed;
        }
        else
        {
            events[eventName] = handle;
        }
    }
    
    public static void On<T>(string eventName, Action<T> handle)
    {
        if (events.TryGetValue(eventName, out var existing))
        {
            var typed = (Action<T>)existing;
            typed += handle;
            events[eventName] = typed;
        }
        else
        {
            events[eventName] = handle;
        }
    }
    
    public static void Emit(string eventName)
    {
        if (events.TryGetValue(eventName, out var del))
        {
            var typed = (Action)del;
            typed.Invoke();
        }
    }

    public static void Emit<T>(string eventName, T arg)
    {
        if (events.TryGetValue(eventName, out var del))
        {
            var typed = (Action<T>)del;
            typed.Invoke(arg);
        }
    }
    
    public static TResult Emit<T, TResult>(string name, T arg)
    {
        if (events.TryGetValue(name, out var del))
        {
            var typed = (Func<T, TResult>)del;
            return typed.Invoke(arg);
        }
        return default;
    }

    public void Off(string eventName)
    {
        events.Remove(eventName);
    }
}
