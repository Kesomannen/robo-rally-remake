using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    void Awake() {
        OverlaySystem.OnOverlayActivated += OnOverlayActivated;
        OverlaySystem.OnOverlayDeactivated += OnOverlayDeactivated;
        gameObject.SetActive(false);
    }

    void OnDestroy() {
        OverlaySystem.OnOverlayActivated -= OnOverlayActivated;
        OverlaySystem.OnOverlayDeactivated -= OnOverlayDeactivated;
    }

    void OnOverlayActivated(OverlayData data) {
        if (data.CanPreview) {
            gameObject.SetActive(true);
        }
    }

    void OnOverlayDeactivated() {
        gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData e) {
        ShowPreview();
    }

    public void OnPointerUp(PointerEventData e) {
        HidePreview();
    }

    void HidePreview() {
        OverlaySystem.Instance.gameObject.SetActive(true);
    }

    void ShowPreview() {
        OverlaySystem.Instance.gameObject.SetActive(false);
    }
}