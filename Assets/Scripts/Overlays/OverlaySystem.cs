using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class OverlaySystem : Singleton<OverlaySystem>, IPointerClickHandler {
    [SerializeField] RectTransform _overlayParent;
    [SerializeField] TMP_Text _headerText, _subtitleText;
    [SerializeField] Image _overlayColorImage;
    [SerializeField] float _overlayFadeTime;
    [SerializeField] Gradient _overlayColorGradient;

    RectTransform _activeOverlayObject;

    public bool IsOverlayActive => _activeOverlayObject != null;

    public static event Action<PointerEventData> OnClick;
    public static event Action<IOverlayData> OnOverlayActivated;
    public static event Action OnOverlayDeactivated;

    protected override void Awake() {
        base.Awake();
        gameObject.SetActive(false);
    }

    public T ShowOverlay<T>(OverlayData<T> data) where T : OverlayBase {
        if (IsOverlayActive) {
            Debug.LogWarning("Overlay already active");
            return null;
        }

        gameObject.SetActive(true);
        
        var obj = Instantiate(data.Prefab, _overlayParent);
        _activeOverlayObject = obj.GetComponent<RectTransform>();

        SetText(data.Header, _headerText);
        SetText(data.Subtitle, _subtitleText);

        OnOverlayActivated?.Invoke(data);

        return obj;

        void SetText(string str, TMP_Text text) {
            if (String.IsNullOrEmpty(str)) {
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

    public void OnPointerClick(PointerEventData e) {
        OnClick?.Invoke(e);
    }
}

public interface IOverlayData {
    string Header { get; }
    string Subtitle { get; }
    bool CanPreview { get; }
}

[Serializable]
public struct OverlayData<T> : IOverlayData where T : Component {
    public string Header { get; set; }
    public string Subtitle { get; set; }
    public T Prefab { get; set; }
    public bool CanPreview { get; set; }
}