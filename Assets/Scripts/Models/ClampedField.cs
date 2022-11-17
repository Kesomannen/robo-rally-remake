using System;

public class ClampedField<T> : IObservableField<T> where T : IComparable<T> {
    readonly T _max;
    readonly T _min;

    public event Action<T, T> OnValueChanged;

    public T Value {
        get => _value;
        set {
            var maxDiff = value.CompareTo(_max);
            var minDiff = value.CompareTo(_min);
            var clampedValue = maxDiff > 0 ? _max : minDiff < 0 ? _min : value;

            if (clampedValue.Equals(_value)) return;
            var prev = _value;
            _value = clampedValue;
            OnValueChanged?.Invoke(prev, _value);
        }
    }

    T _value;

    public ClampedField(T initialValue, T max, T min) {
        _value = initialValue;
        _max = max;
        _min = min;
    }

    public override string ToString(){
        return _value.ToString();
    }
}