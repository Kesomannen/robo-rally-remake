﻿using System;
using UnityEngine;

[Serializable]
public struct Optional<T> {
    [SerializeField] bool _enabled;
    [SerializeField] T _value;
    
    public Optional(T initialValue) {
        _enabled = true;
        _value = initialValue;
    }

    public bool Enabled => _enabled;
    public T Value => _value;
}