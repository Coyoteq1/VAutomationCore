using System;
using System.Collections.Generic;

namespace VAutomationCore.Core.Events
{
    /// <summary>
    /// Lightweight in-process typed event bus.
    /// </summary>
    public static class TypedEventBus
    {
        private static readonly object Sync = new();
        private static readonly Dictionary<Type, List<Delegate>> Handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            lock (Sync)
            {
                var type = typeof(T);
                if (!Handlers.TryGetValue(type, out var list))
                {
                    list = new List<Delegate>();
                    Handlers[type] = list;
                }

                list.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            lock (Sync)
            {
                var type = typeof(T);
                if (!Handlers.TryGetValue(type, out var list))
                {
                    return;
                }

                list.Remove(handler);
                if (list.Count == 0)
                {
                    Handlers.Remove(type);
                }
            }
        }

        public static void Publish<T>(T evt)
        {
            Delegate[] invocationList;
            lock (Sync)
            {
                if (!Handlers.TryGetValue(typeof(T), out var list) || list.Count == 0)
                {
                    return;
                }

                invocationList = list.ToArray();
            }

            for (var i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action<T>)invocationList[i]).Invoke(evt);
                }
                catch
                {
                    // Event handlers are isolated.
                }
            }
        }

        public static void ClearAll()
        {
            lock (Sync)
            {
                Handlers.Clear();
            }
        }
    }
}
