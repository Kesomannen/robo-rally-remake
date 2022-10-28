using System;

public class ObservableField<T> : IObservableField<T> {
    public event Action<T, T> OnValueChange;

    public T Value {
        get => _value;
        set {
            if (value.Equals(_value)) return;
            var prev = _value;
            _value = value;
            OnValueChange?.Invoke(prev, _value);
        }
    }

    T _value;

    public ObservableField(T initialValue)
    {
        _value = initialValue;
    }
}