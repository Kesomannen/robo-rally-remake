using System;

public interface IObservableField<T> {
    T Value { get; set; }
    event Action<T, T> OnValueChange;
}