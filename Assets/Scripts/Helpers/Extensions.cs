using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Extensions {
    public static void Shuffle<T>(this IList<T> list) {
        for (var i = list.Count - 1; i > 0; i--) {
            var j = Random.Range(0, i + 1);
            (list[j], list[i]) = (list[i], list[j]);
        }
    }

    public static int IndexOf<T>(this IReadOnlyList<T> list, T item) {
        for (var i = 0; i < list.Count; i++) {
            if (list[i] != null && list[i].Equals(item)) {
                return i;
            }
        }
        return -1;
    }
    
    public static int IndexOf<T>(this IReadOnlyList<T> list, Func<T, bool> predicate) {
        for (var i = 0; i < list.Count; i++) {
            if (list[i] != null && predicate(list[i])) {
                return i;
            }
        }
        return -1;
    }
    
    public static T GetRandom<T>(this IReadOnlyList<T> list) => list[Random.Range(0, list.Count)];

    public static TValue EnforceKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) {
        if (!dictionary.ContainsKey(key)) dictionary.Add(key, defaultValue);
        return dictionary[key];
    }

    public static TValue EnforceKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValue) {
        if (!dictionary.ContainsKey(key)) dictionary.Add(key, defaultValue());
        return dictionary[key];
    }

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TKey> list, Func<TKey, TValue> valueSelector) {
        return list.ToDictionary(key => key, valueSelector);
    }
    
    public static void SetActive(this Component component, bool active) => component.gameObject.SetActive(active);
}