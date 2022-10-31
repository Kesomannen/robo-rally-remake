using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Choice<T> : OverlayBase {
    protected ChoiceCallbackReciever _callbackReciever;
    protected T[] _options;

    public abstract int MaxOptions { get; }
    public abstract int MinOptions { get; }

    public delegate void ChoiceCallbackReciever(ChoiceData choiceData);

    public virtual void Init(T[] options, ChoiceCallbackReciever callback) {
        if (options.Length > MaxOptions || options.Length < MinOptions) {
            Debug.LogError($"Number of options ({options.Length}) is out of range ({MinOptions} - {MaxOptions})");
            return;
        }

        _options = options;
        _callbackReciever = callback;
    }

    protected override void OnOverlayClick(PointerEventData e) {
        base.OnOverlayClick(e);
        Cancel();
    }

    public void OnOptionChoose(T choice) {
        _callbackReciever?.Invoke(new() {
            Choice = choice,
            WasCanceled = false
        });
    }

    public void Cancel() {
        _callbackReciever?.Invoke(new() {
            Choice = default,
            WasCanceled = true
        });
    }

    public struct ChoiceData {
        public T Choice;
        public bool WasCanceled;
    }
}