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

        /// <summary>
        /// Subscribes to an event and returns a disposable unsubscriber.
        /// </summary>
        public static IDisposable SubscribeScoped<T>(Action<T> handler)
        {
            Subscribe(handler);
            return new ScopedSubscription<T>(handler);
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
            PublishAndCount(evt);
        }

        /// <summary>
        /// Publishes an event and returns the number of handlers that ran successfully.
        /// </summary>
        public static int PublishAndCount<T>(T evt)
        {
            Delegate[] invocationList;
            lock (Sync)
            {
                if (!Handlers.TryGetValue(typeof(T), out var list) || list.Count == 0)
                {
                    return 0;
                }

                invocationList = list.ToArray();
            }

            var successCount = 0;
            for (var i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action<T>)invocationList[i]).Invoke(evt);
                    successCount++;
                }
                catch
                {
                    // Event handlers are isolated.
                }
            }

            return successCount;
        }

        /// <summary>
        /// Gets subscriber count for a specific event type.
        /// </summary>
        public static int GetSubscriberCount<T>()
        {
            lock (Sync)
            {
                return Handlers.TryGetValue(typeof(T), out var list) ? list.Count : 0;
            }
        }

        /// <summary>
        /// Returns true when at least one subscriber is registered for event type T.
        /// </summary>
        public static bool HasSubscribers<T>()
        {
            return GetSubscriberCount<T>() > 0;
        }

        public static void ClearAll()
        {
            lock (Sync)
            {
                Handlers.Clear();
            }
        }

        private sealed class ScopedSubscription<T> : IDisposable
        {
            private readonly Action<T> _handler;
            private bool _disposed;

            public ScopedSubscription(Action<T> handler)
            {
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                Unsubscribe(_handler);
                _disposed = true;
            }
        }
    }
}
