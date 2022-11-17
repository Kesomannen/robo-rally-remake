using System;
using UnityEngine.EventSystems;

public abstract class Choice<T> : OverlayBase {
    Action<ChoiceResult> _callback;
    protected T[] Options;

    protected abstract int MaxOptions { get; }
    protected abstract int MinOptions { get; }

    bool _isOptional;

    protected void Init(T[] options, Action<ChoiceResult> callback, bool isOptional = false) {
        if (options.Length > MaxOptions || options.Length < MinOptions) {
            throw new ArgumentOutOfRangeException($"Options must be between {MinOptions} and {MaxOptions}.");
        }

        Options = options;
        _callback = callback;
        _isOptional = isOptional;
    }

    protected override void OnOverlayClick(PointerEventData e){
        if (!_isOptional) return;
        base.OnOverlayClick(e);
        Cancel();
    }

    protected void OnOptionChoose(T choice) {
        _callback?.Invoke(new ChoiceResult {
            Choice = choice,
            WasCanceled = false
        });
        OverlaySystem.Instance.HideOverlay();
    }

    protected void Cancel() {
        if (!_isOptional) return;
        _callback?.Invoke(new ChoiceResult {
            Choice = default,
            WasCanceled = true
        });
        OverlaySystem.Instance.HideOverlay();
    }

    public struct ChoiceResult {
        public T Choice;
        public bool WasCanceled;
    }
}