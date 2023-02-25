using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class OverlaySystem : Singleton<OverlaySystem>, IPointerClickHandler {
    [Header("Input")]
    [SerializeField] InputAction _exitAction;
    
    [Header("References")]
    [SerializeField] RectTransform _overlayParent;
    [SerializeField] TMP_Text _headerText, _subtitleText;
    
    readonly Stack<(OverlayData Data, Overlay Overlay)> _overlayStack = new();
    Overlay _currentOverlay;

    bool IsOverlayActive => _overlayStack.Count > 0;

    public static event Action OnClick;
    public static event Action<OverlayData> OnOverlayShown, OnOverlayHidden;

    protected override void Awake() {
        base.Awake();
        gameObject.SetActive(false);
    }

    void OnEnable() {
        _exitAction.performed += OnExitAction;
        _exitAction.Enable();
    }
    
    void OnDisable() {
        _exitAction.performed -= OnExitAction;
        _exitAction.Disable();
    }
    
    public void OnPointerClick(PointerEventData e) {
        OnExitAction();
    }
    
    static void OnExitAction(InputAction.CallbackContext c = default) {
        OnClick?.Invoke();
    }

    public T PushAndShowOverlay<T>(OverlayData<T> data) where T : Overlay {
        var newOverlay = Instantiate(data._prefab, _overlayParent);
        _overlayStack.Push((data, newOverlay));
        
        ShowTopOverlay();
        return newOverlay;
    }

    void ShowTopOverlay() {
        // If this is the first overlay, show the overlay system
        if (_overlayStack.Count == 1) {
            gameObject.SetActive(true);
        } else {
            // Hide current top overlay
            _currentOverlay.SetActive(false);
        }

        var (data, overlay) = _overlayStack.Peek();
        _currentOverlay = overlay;
        overlay.SetActive(true);
        
        SetText(data._header, _headerText);
        SetText(data._subtitle, _subtitleText);

        OnOverlayShown?.Invoke(data);

        void SetText(string str, TMP_Text text) {
            if (string.IsNullOrEmpty(str)) {
                text.SetActive(false);
            } else {
                text.SetActive(true);
                text.text = str;
            }
        }
    }

    public void DestroyCurrentOverlay() {
        if (!IsOverlayActive) {
            Debug.LogWarning("No overlay active");
            return;
        }

        var (data, obj) = _overlayStack.Pop();
        Destroy(obj.gameObject);
        
        if (IsOverlayActive) {
            ShowTopOverlay();
        } else {
            // That was the last overlay
            gameObject.SetActive(false);
            _headerText.gameObject.SetActive(false);
            _subtitleText.gameObject.SetActive(false);
        }

        OnOverlayHidden?.Invoke(data);  
    }
}

[Serializable]
public struct OverlayData {
    [FormerlySerializedAs("Header")]
    public string _header;
    [FormerlySerializedAs("Subtitle")] 
    public string _subtitle;
    [FormerlySerializedAs("Prefab")] 
    public Overlay _prefab;
    [FormerlySerializedAs("CanPreview")] 
    public bool _canPreview;
}

[Serializable]
public struct OverlayData<T> where T : Overlay {
    [FormerlySerializedAs("Header")]
    public string _header;
    [FormerlySerializedAs("Subtitle")]
    public string _subtitle;
    [FormerlySerializedAs("Prefab")]
    public T _prefab;
    [FormerlySerializedAs("CanPreview")]
    public bool _canPreview;

    public static implicit operator OverlayData(OverlayData<T> data) => new() {
        _header = data._header,
        _subtitle = data._subtitle,
        _prefab = data._prefab,
        _canPreview = data._canPreview
    };
}