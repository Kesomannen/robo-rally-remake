using System;
using System.Collections.Generic;
using UnityEngine;

public class LookupTable : ScriptableObject {
    [SerializeField] ScriptableObject[] _values;
    [SerializeField] int _maxDerivations = 2;

    public ScriptableObject this[int id] => _values[id];
    public IReadOnlyList<ScriptableObject> Values => _values;
    
    public Type Type { get; private set; }

    void OnValidate() {
        if (_values.Length == 0) return;
        Type = _values[0].GetType();

        for (var i = 0; i < _values.Length; i++) {
            if (_values[i] == null) continue;
            var type = Type;
            var derivations = 0;
            var isValid = false;
            
            while (derivations < _maxDerivations && type.BaseType != null) {
                if (type.IsInstanceOfType(_values[i])) {
                    isValid = true;
                    break;
                }

                type = type.BaseType;
                derivations++;
            }

            if (!isValid) {
                Debug.LogError($"Lookup table {name} contains a value of type {_values[i].GetType()} at index {i}, but should only contain values of type {Type} or its derivatives.");
            }
        }
    }
}