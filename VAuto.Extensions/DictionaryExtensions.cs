using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VAuto.Extensions
{
    /// <summary>
    /// Generic Dictionary extension methods
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Get string value from dictionary
        /// </summary>
        public static string GetString(this Dictionary<string, object> dict, string key, string defaultValue = "")
        {
            if (dict.TryGetValue(key, out var val))
                return val?.ToString() ?? defaultValue;
            return defaultValue;
        }

        /// <summary>
        /// Get int value from dictionary
        /// </summary>
        public static int GetInt(this Dictionary<string, object> dict, string key, int defaultValue = 0)
        {
            if (dict.TryGetValue(key, out var val))
            {
                if (val is int i) return i;
                if (int.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get long value from dictionary
        /// </summary>
        public static long GetLong(this Dictionary<string, object> dict, string key, long defaultValue = 0)
        {
            if (dict.TryGetValue(key, out var val))
            {
                if (val is long l) return l;
                if (long.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get bool value from dictionary
        /// </summary>
        public static bool GetBool(this Dictionary<string, object> dict, string key, bool defaultValue = false)
        {
            if (dict.TryGetValue(key, out var val))
            {
                if (val is bool b) return b;
                if (bool.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get float value from dictionary
        /// </summary>
        public static float GetFloat(this Dictionary<string, object> dict, string key, float defaultValue = 0f)
        {
            if (dict.TryGetValue(key, out var val))
            {
                if (val is float f) return f;
                if (val is double d) return (float)d;
                if (float.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get double value from dictionary
        /// </summary>
        public static double GetDouble(this Dictionary<string, object> dict, string key, double defaultValue = 0.0)
        {
            if (dict.TryGetValue(key, out var val))
            {
                if (val is double d) return d;
                if (val is float f) return f;
                if (double.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get typed object from dictionary using JSON
        /// </summary>
        public static T? GetObject<T>(this Dictionary<string, object> dict, string key)
        {
            if (!dict.TryGetValue(key, out var val)) return default;
            var json = JsonConvert.SerializeObject(val);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Get or default from dictionary
        /// </summary>
        public static T GetOrDefault<T>(this Dictionary<string, object> dict, string key, T defaultValue)
        {
            return dict.GetObject<T>(key) ?? defaultValue;
        }

        /// <summary>
        /// Add value if not exists
        /// </summary>
        public static void AddIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
            where TKey : notnull
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, value);
        }

        /// <summary>
        /// Get or add value
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
            where TKey : notnull
        {
            if (dict.TryGetValue(key, out var existing))
                return existing;
            dict.Add(key, value);
            return value;
        }

        /// <summary>
        /// Get or add value using factory
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> factory)
            where TKey : notnull
        {
            if (dict.TryGetValue(key, out var existing))
                return existing;
            
            var value = factory(key);
            dict.Add(key, value);
            return value;
        }

        /// <summary>
        /// Try get value and return success
        /// </summary>
        public static bool TryGet<T>(this Dictionary<string, object> dict, string key, out T? value)
        {
            if (dict.TryGetValue(key, out var val))
            {
                try
                {
                    if (val is T typedVal)
                    {
                        value = typedVal;
                        return true;
                    }
                    var json = JsonConvert.SerializeObject(val);
                    value = JsonConvert.DeserializeObject<T>(json);
                    return value != null;
                }
                catch
                {
                    value = default;
                    return false;
                }
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Merge two dictionaries
        /// </summary>
        public static Dictionary<string, object> Merge(this Dictionary<string, object> dict, Dictionary<string, object> other)
        {
            var result = new Dictionary<string, object>(dict);
            foreach (var kvp in other)
            {
                result[kvp.Key] = kvp.Value;
            }
            return result;
        }

        /// <summary>
        /// Deep clone dictionary using JSON
        /// </summary>
        public static Dictionary<string, object> DeepClone(this Dictionary<string, object> dict)
        {
            var json = JsonConvert.SerializeObject(dict);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Filter dictionary by keys
        /// </summary>
        public static Dictionary<string, object> FilterByKeys(this Dictionary<string, object> dict, IEnumerable<string> keys)
        {
            var keySet = new HashSet<string>(keys);
            var result = new Dictionary<string, object>();
            foreach (var kvp in dict)
            {
                if (keySet.Contains(kvp.Key))
                    result[kvp.Key] = kvp.Value;
            }
            return result;
        }

        /// <summary>
        /// Convert dictionary to object
        /// </summary>
        public static T ToObject<T>(this Dictionary<string, object> dict) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(dict)) ?? new T();
        }
    }
}