using System;
using System.Collections.Generic;
using Air.UnityGameCore.Runtime.Event;
using UnityEngine;

namespace Air.UI
{
    /// <summary>
    /// UI 缁勪欢绾т簨浠朵綔鐢ㄥ煙锛氭敞鍐屽埌 <see cref="EventBus"/> 骞惰窡韪敞閿€銆?    /// </summary>
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
                Debug.LogWarning($"[UIScopedEvents] 鏈壘鍒板凡娉ㄥ唽浜嬩欢: {eventName}");
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
                Debug.LogError("[UIScopedEvents] 浜嬩欢鍚嶇О涓虹┖");
                return false;
            }
            if (callback == null)
            {
                Debug.LogError("[UIScopedEvents] 鍥炶皟涓虹┖");
                return false;
            }
            if (_unregister.ContainsKey(eventName))
            {
                Debug.LogError($"[UIScopedEvents] 宸叉敞鍐岃繃璇ヤ簨浠? {eventName}");
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
