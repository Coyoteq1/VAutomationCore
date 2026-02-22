using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VAutomationCore.Core.Api
{
    /// <summary>
    /// Type-safe runtime service registry for cross-module APIs.
    /// </summary>
    public static class ServiceRegistry
    {
        private static readonly ConcurrentDictionary<Type, object> Services = new();

        /// <summary>
        /// Registers a singleton service instance.
        /// </summary>
        public static bool RegisterSingleton<TService>(TService instance, bool replace = false) where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var key = typeof(TService);
            if (replace)
            {
                Services[key] = instance;
                return true;
            }

            return Services.TryAdd(key, instance);
        }

        /// <summary>
        /// Attempts to resolve a registered service.
        /// </summary>
        public static bool TryResolve<TService>(out TService service) where TService : class
        {
            if (Services.TryGetValue(typeof(TService), out var boxed) && boxed is TService typed)
            {
                service = typed;
                return true;
            }

            service = null!;
            return false;
        }

        /// <summary>
        /// Resolves a registered service or throws when missing.
        /// </summary>
        public static TService GetRequired<TService>() where TService : class
        {
            if (TryResolve<TService>(out var service))
            {
                return service;
            }

            throw new InvalidOperationException($"Service '{typeof(TService).FullName}' is not registered.");
        }

        /// <summary>
        /// Returns true when service type TService is currently registered.
        /// </summary>
        public static bool IsRegistered<TService>() where TService : class
        {
            return Services.ContainsKey(typeof(TService));
        }

        /// <summary>
        /// Removes a service registration.
        /// </summary>
        public static bool Remove<TService>() where TService : class
        {
            return Services.TryRemove(typeof(TService), out _);
        }

        /// <summary>
        /// Clears all service registrations.
        /// </summary>
        public static void Clear()
        {
            Services.Clear();
        }

        /// <summary>
        /// Gets the number of currently registered services.
        /// </summary>
        public static int Count => Services.Count;

        /// <summary>
        /// Returns a snapshot of currently registered service types.
        /// </summary>
        public static IReadOnlyCollection<Type> GetRegisteredTypes()
        {
            return new List<Type>(Services.Keys);
        }
    }
}
