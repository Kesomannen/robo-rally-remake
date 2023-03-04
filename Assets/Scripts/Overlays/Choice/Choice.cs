using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Choice<T> : Overlay {
    [SerializeField] Button _submitButton;
    [SerializeField] [ReadOnly] int _minChoices;
    [SerializeField] [ReadOnly] int _maxChoices;
    [SerializeField] [ReadOnly] bool _canCancel;

    public bool IsDone { get; private set; }
    public bool WasCancelled { get; private set; }
    public IReadOnlyList<int> SelectedOptions => _selectedOptions;
    
    protected IReadOnlyList<T> Options { get; private set; }
    protected Func<T, bool> AvailablePredicate { get; private set; }
    readonly List<int> _selectedOptions = new();

    bool _ignoreSubmit;
    bool ValidInput => _selectedOptions.Count >= _minChoices && _selectedOptions.Count <= _maxChoices;
    
    public event Action<IEnumerable<int>> OnSubmit;
    public event Action OnCancel;

    protected abstract void OnInit();

    public void Init(IReadOnlyList<T> options, bool canCancel, int minChoices = 1, int maxChoices = 1, Func<T, bool> availablePredicate = null) {
        Options = options;
        AvailablePredicate = availablePredicate ?? (_ => true);
        _minChoices = minChoices;
        _maxChoices = maxChoices;
        _canCancel = canCancel;

        _ignoreSubmit = _maxChoices == 1;
        _submitButton.gameObject.SetActive(!_ignoreSubmit);
        if (!_ignoreSubmit) {
            _submitButton.onClick.AddListener(Submit);
            _submitButton.interactable = false;
        }

        OnInit();
    }

    protected void Toggle(int option) {
        if (_selectedOptions.Contains(option)) {
            _selectedOptions.Remove(option);
        } else {
            _selectedOptions.Add(option);
            if (_ignoreSubmit) {
                Submit();
            }
        }
        _submitButton.interactable = ValidInput;
    }

    void Submit() {
        if (!ValidInput) return;
        
        IsDone = true;
        OnSubmit?.Invoke(_selectedOptions);

        OverlaySystem.Instance.DestroyCurrentOverlay();
    }

    protected override void OnOverlayClick() {
        if (!_canCancel) return;
        
        IsDone = true;
        WasCancelled = true;
        OnCancel?.Invoke();

        OverlaySystem.Instance.DestroyCurrentOverlay();
    }
}