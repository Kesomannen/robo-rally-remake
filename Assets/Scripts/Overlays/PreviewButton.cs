using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
    bool _isPreviewing;

    void Awake() {
        OverlaySystem.OnOverlayActivated += OnOverlayActivated;
    }

    void OnDestroy() {
        OverlaySystem.OnOverlayActivated -= OnOverlayActivated;
    }

    void OnOverlayActivated(IOverlayData obj) {
        if (obj.CanPreview) {
            gameObject.SetActive(true);
        }
    }

    public void OnPointerDown(PointerEventData e) {
        ShowPreview();
    }

    public void OnPointerUp(PointerEventData e) {
        HidePreview();
    }

    public void OnPointerExit(PointerEventData e) {
        if (_isPreviewing) {
            ShowPreview();
        }
    }

    void HidePreview() {
        _isPreviewing = false;
        OverlaySystem.Instance.gameObject.SetActive(true);
    }

    void ShowPreview() {
        _isPreviewing = true;
        OverlaySystem.Instance.gameObject.SetActive(false);
    }
}