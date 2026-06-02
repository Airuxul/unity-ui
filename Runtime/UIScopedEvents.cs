using System;
using System.Collections.Generic;
using Air.UnityGameCore.Runtime.Event;
using UnityEngine;

namespace Air.UI
{
    /// <summary>
    /// UI 组件级事件作用域：注册到 <see cref="EventBus"/> 并跟踪注销。
    /// </summary>
    public sealed class UIScopedEvents
    {
        readonly EventBus _bus;
        readonly Dictionary<string, Action> _unregister = new();

        public UIScopedEvents(EventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public void On(string eventName, Action callback)
        {
            if (!TryRegister(eventName, callback)) return;
            _bus.On(eventName, callback);
            _unregister[eventName] = () => _bus.Off(eventName, callback);
        }

        public void On<T>(string eventName, Action<T> callback)
        {
            if (!TryRegister(eventName, callback)) return;
            _bus.On(eventName, callback);
            _unregister[eventName] = () => _bus.Off(eventName, callback);
        }

        public void Emit(string eventName) => _bus.Emit(eventName);

        public void Emit<T>(string eventName, T payload) => _bus.Emit(eventName, payload);

        public void Off(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            if (!_unregister.TryGetValue(eventName, out var unregister))
            {
                Debug.LogWarning($"[UIScopedEvents] 未找到已注册事件: {eventName}");
                return;
            }
            unregister.Invoke();
            _unregister.Remove(eventName);
        }

        public void Clear()
        {
            foreach (var unregister in _unregister.Values)
                unregister?.Invoke();
            _unregister.Clear();
        }

        bool TryRegister(string eventName, Delegate callback)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogError("[UIScopedEvents] 事件名称为空");
                return false;
            }
            if (callback == null)
            {
                Debug.LogError("[UIScopedEvents] 回调为空");
                return false;
            }
            if (_unregister.ContainsKey(eventName))
            {
                Debug.LogError($"[UIScopedEvents] 已注册过该事件: {eventName}");
                return false;
            }
            return true;
        }
    }

    public interface IUIScopedEvents
    {
        UIScopedEvents ScopedEvents { get; }
    }

    public static class UIScopedEventsExtensions
    {
        public static void On(this IUIScopedEvents target, string eventName, Action action) =>
            target.ScopedEvents.On(eventName, action);

        public static void On<T>(this IUIScopedEvents target, string eventName, Action<T> action) =>
            target.ScopedEvents.On(eventName, action);

        public static void Emit(this IUIScopedEvents target, string eventName) =>
            target.ScopedEvents.Emit(eventName);

        public static void Emit<T>(this IUIScopedEvents target, string eventName, T payload) =>
            target.ScopedEvents.Emit(eventName, payload);

        public static void Off(this IUIScopedEvents target, string eventName) =>
            target.ScopedEvents.Off(eventName);

        public static void ClearEvents(this IUIScopedEvents target) =>
            target.ScopedEvents.Clear();
    }
}