using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Choice<T> : Overlay {
    [SerializeField] Button _submitButton;
    [SerializeField] [ReadOnly] int _minChoices;
    [SerializeField] [ReadOnly] int _maxChoices;
    [SerializeField] [ReadOnly] bool _canCancel;
    
    public bool IsCancelled { get; private set; }
    public bool IsSubmitted { get; private set; }
    public IReadOnlyList<int> SelectedOptions => _selectedOptions;

    protected IReadOnlyList<T> Options { get; private set; }
    protected Func<T, bool> AvailablePredicate { get; private set; }
    readonly List<int> _selectedOptions = new();

    bool _ignoreSubmit;
    
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
    }

    void Submit() {
        if (_selectedOptions.Count < _minChoices || _selectedOptions.Count > _maxChoices) return;
        
        OnSubmit?.Invoke(_selectedOptions);
        IsSubmitted = true;
        
        OverlaySystem.Instance.DestroyCurrentOverlay();
    }

    protected override void OnOverlayClick() {
        if (!_canCancel) return;
        
        OnCancel?.Invoke();
        IsCancelled = true;
        
        OverlaySystem.Instance.DestroyCurrentOverlay();
    }
}