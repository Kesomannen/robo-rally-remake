using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class OverlaySystem : Singleton<OverlaySystem>, IPointerClickHandler {
    [Header("Input")]
    [SerializeField] InputAction _exitAction;
    
    [Header("References")]
    [SerializeField] RectTransform _overlayParent;
    [SerializeField] TMP_Text _headerText, _subtitleText;

    RectTransform _activeOverlayObject;

    bool IsOverlayActive => _activeOverlayObject != null;

    public static event Action OnClick;
    public static event Action<OverlayData> OnOverlayActivated;
    public static event Action OnOverlayDeactivated;

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

    public T ShowOverlay<T>(OverlayData<T> data) where T : Overlay {
        if (IsOverlayActive) return null;

        gameObject.SetActive(true);
        
        var obj = Instantiate(data.Prefab, _overlayParent);
        _activeOverlayObject = obj.GetComponent<RectTransform>();

        SetText(data.Header, _headerText);
        SetText(data.Subtitle, _subtitleText);

        OnOverlayActivated?.Invoke(data);

        return obj;

        void SetText(string str, TMP_Text text) {
            if (string.IsNullOrEmpty(str)) {
                text.gameObject.SetActive(false);
            } else {
                text.gameObject.SetActive(true);
                text.text = str;
            }
        }
    }

    public void HideOverlay() {
        if (!IsOverlayActive) {
            Debug.LogWarning("No overlay active");
            return;
        }

        gameObject.SetActive(false);

        Destroy(_activeOverlayObject.gameObject);
        _activeOverlayObject = null;

        _headerText.gameObject.SetActive(false);
        _subtitleText.gameObject.SetActive(false);

        OnOverlayDeactivated?.Invoke();        
    }
}

[Serializable]
public struct OverlayData {
    public string Header;
    public string Subtitle;
    public Overlay Prefab;
    public bool CanPreview;
}

[Serializable]
public struct OverlayData<T> where T : Overlay {
    public string Header;
    public string Subtitle;
    public T Prefab;
    public bool CanPreview;

    public static implicit operator OverlayData(OverlayData<T> data) => new() {
        Header = data.Header,
        Subtitle = data.Subtitle,
        Prefab = data.Prefab,
        CanPreview = data.CanPreview
    };
}