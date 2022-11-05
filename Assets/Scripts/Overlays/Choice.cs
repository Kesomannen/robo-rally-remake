using System;
using UnityEngine.EventSystems;

public abstract class Choice<T> : OverlayBase {
    protected Action<ChoiceResult> Callback;
    protected T[] Options;

    public abstract int MaxOptions { get; }
    public abstract int MinOptions { get; }

    bool _isOptional;

    protected void Init(T[] options, Action<ChoiceResult> callback, bool isOptional = false) {
        if (options.Length > MaxOptions || options.Length < MinOptions) {
            throw new ArgumentOutOfRangeException($"Options must be between {MinOptions} and {MaxOptions}.");
        }

        Options = options;
        Callback = callback;
        _isOptional = isOptional;
    }

    protected override void OnOverlayClick(PointerEventData e) {
        if (_isOptional) {
            base.OnOverlayClick(e);
            Cancel();
        }
    }

    public void OnOptionChoose(T choice) {
        Callback?.Invoke(new() {
            Choice = choice,
            WasCanceled = false
        });
        OverlaySystem.Instance.HideOverlay();
    }

    public void Cancel() {
        if (!_isOptional) return;
        Callback?.Invoke(new() {
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