using System;

namespace VAutomationCore.Core.Events
{
    /// <summary>
    /// Stable event facade for engine-to-gameplay communication.
    /// </summary>
    public static class EventDispatcher
    {
        public static IDisposable Subscribe<T>(Action<T> handler)
        {
            return TypedEventBus.SubscribeScoped(handler);
        }

        public static void Publish<T>(T evt)
        {
            TypedEventBus.Publish(evt);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            TypedEventBus.Unsubscribe(handler);
        }

        public static bool HasSubscribers<T>()
        {
            return TypedEventBus.HasSubscribers<T>();
        }
    }
}