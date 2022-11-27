using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct StatEvent<T> {
    readonly List<Func<T, T>> _funcList;
    readonly T _initialValue;

    public StatEvent(T initialValue) {
        _funcList = new List<Func<T, T>>();
        _initialValue = initialValue;
    }

    public void Add(Func<T, T> func) {
        _funcList.Add(func);
    }
    
    public void Add(T value) {
        _funcList.Add(x => value);
    }
    
    public bool Remove(Func<T, T> func) {
        return _funcList.Remove(func);
    }
    
    public bool Remove(T value) {
        return _funcList.Remove(x => value);
    }

    public T Invoke() {
        return _funcList.Aggregate(_initialValue, (current, func) => func(current));
    }
}