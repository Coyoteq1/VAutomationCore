using System.Collections.Generic;
using System.Linq;

namespace VAuto.Extensions
{
    /// <summary>
    /// Generic collection extension methods
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Convert enumerable to comma-separated string
        /// </summary>
        public static string JoinString<T>(this IEnumerable<T> collection, string separator = ", ")
        {
            return string.Join(separator, collection);
        }

        /// <summary>
        /// Safe list access with default
        /// </summary>
        public static T GetOrDefault<T>(this IList<T> list, int index, T defaultValue = default!)
        {
            return index >= 0 && index < list.Count ? list[index] : defaultValue;
        }

        /// <summary>
        /// Shuffle collection using Fisher-Yates algorithm
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
        {
            var random = new Random();
            var list = collection.ToList();
            
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            
            return list;
        }

        /// <summary>
        /// Check if collection is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// ForEach extension for collections
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        /// <summary>
        /// ForEach with index
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            int index = 0;
            foreach (var item in collection)
            {
                action(item, index++);
            }
        }

        /// <summary>
        /// DistinctBy - return distinct elements by key selector
        /// </summary>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var item in collection)
            {
                if (seenKeys.Add(keySelector(item)))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Chunk collection into smaller lists
        /// </summary>
        public static IEnumerable<List<T>> Chunk<T>(this IEnumerable<T> collection, int size)
        {
            var chunk = new List<T>(size);
            foreach (var item in collection)
            {
                chunk.Add(item);
                if (chunk.Count == size)
                {
                    yield return chunk;
                    chunk = new List<T>(size);
                }
            }
            if (chunk.Count > 0)
            {
                yield return chunk;
            }
        }

        /// <summary>
        /// Get first or default with predicate
        /// </summary>
        public static T? FirstOrDefault<T>(this IEnumerable<T> collection, Func<T, bool> predicate, T? defaultValue = default)
        {
            foreach (var item in collection)
            {
                if (predicate(item))
                    return item;
            }
            return defaultValue;
        }

        /// <summary>
        /// Add range to collection
        /// </summary>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Remove all matching items
        /// </summary>
        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            var toRemove = collection.Where(predicate).ToList();
            foreach (var item in toRemove)
            {
                collection.Remove(item);
            }
        }

        /// <summary>
        /// Get all indices matching predicate
        /// </summary>
        public static IEnumerable<int> AllIndicesOf<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    yield return i;
            }
        }
    }
}