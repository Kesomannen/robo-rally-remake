using System.Collections.Generic;
using UnityEngine;

public class LookupTable : ScriptableObject {
    [SerializeField] ScriptableObject[] _values;

    public ScriptableObject this[int id] => _values[id];
    public IReadOnlyList<ScriptableObject> Values => _values;
}