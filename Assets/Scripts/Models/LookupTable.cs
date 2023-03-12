using System.Collections.Generic;
using UnityEngine;

public abstract class LookupTable<T> : ScriptableObject where T : ScriptableObject {
    [SerializeField] T[] _values;

    public T this[int id] => _values[id];
    public IReadOnlyList<T> Values => _values;
}