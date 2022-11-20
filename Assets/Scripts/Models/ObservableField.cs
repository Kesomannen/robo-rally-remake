using System;

public class ObservableField<T> : IObservableField<T> {
    public event Action<T, T> OnValueChanged;

    public T Value {
        get => _value;
        set {
            if (value.Equals(_value)) return;
            var prev = _value;
            _value = value;
            OnValueChanged?.Invoke(prev, _value);
        }
    }

    T _value;

    public ObservableField(T initialValue) {
        _value = initialValue;
    }

    public override string ToString() {
        return _value.ToString();
    }
}