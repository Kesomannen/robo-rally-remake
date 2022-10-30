using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

public class OverlaySystem : Singleton<OverlaySystem>, IPointerClickHandler {
    [SerializeField] RectTransform _overlayParent;
    [SerializeField] TMP_Text _headerText, _subTitleText;
    [SerializeField] Image _overlayColorImage;
    [SerializeField] float _overlayFadeTime = 0.5f;
    [SerializeField] Gradient _overlayColorGradient;

    RectTransform _activeOverlayObject;

    public bool IsOverlayActive => _activeOverlayObject != null;

    public static event Action<PointerEventData> OnClick;

    public T ShowOverlay<T>(T prefab, string header = null, string subTitle = null) where T : Component {
        if (_activeOverlayObject != null) {
            Debug.LogWarning("Overlay already active");
            return null;
        }

        gameObject.SetActive(true);
        var obj = Instantiate(prefab, _overlayParent);
        _activeOverlayObject = obj.GetComponent<RectTransform>();

        Fade(_overlayParent, true);
        Fade(_activeOverlayObject, true);

        SetText(header, _headerText);
        SetText(subTitle, _subTitleText);

        return obj;

        void SetText(string str, TMP_Text text) {
            if (String.IsNullOrEmpty(str)) {
                text.gameObject.SetActive(false);
            } else {
                text.gameObject.SetActive(true);
                text.text = str;
                Fade(text.GetComponent<RectTransform>(), true);
            }
        }
    }

    public void HideOverlay() {
        if (_activeOverlayObject == null) {
            Debug.LogWarning("No overlay active");
            return;
        }
        
        Fade(_activeOverlayObject, false).setOnComplete(() => {
            Destroy(_activeOverlayObject.gameObject);
            _activeOverlayObject = null;
        });

        Fade(_overlayParent, false).setOnComplete(() => {
            gameObject.SetActive(false);
        });

        if (_headerText.gameObject.activeSelf) {
            Fade(_headerText.GetComponent<RectTransform>(), false);
        }

        if (_subTitleText.gameObject.activeSelf) {
            Fade(_subTitleText.GetComponent<RectTransform>(), false);
        }
    }

    LTDescr Fade(RectTransform target, bool fadeIn) {
        var startAlpha = fadeIn ? 0 : 1;
        var targetAlpha = fadeIn ? 1 : 0;
        return LeanTween.alpha(_activeOverlayObject, targetAlpha, _overlayFadeTime).setFrom(startAlpha);
    }

    public void OnPointerClick(PointerEventData e) {
        OnClick?.Invoke(e);
    }
}