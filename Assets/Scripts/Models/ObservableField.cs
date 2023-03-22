using System;

public class ObservableField<T> {
    public event Action<T, T> ValueChanged;

    public T Value {
        get => _value;
        set {
            if (value.Equals(_value)) return;
            var prev = _value;
            _value = value;
            ValueChanged?.Invoke(prev, _value);
        }
    }

    T _value;

    public ObservableField(T initialValue = default) {
        _value = initialValue;
    }

    public override string ToString() => _value.ToString();
}