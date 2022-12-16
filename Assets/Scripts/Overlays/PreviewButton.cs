using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    void Awake() {
        OverlaySystem.OnOverlayShown += OnOverlayShown;
        OverlaySystem.OnOverlayHidden += OnOverlayHidden;
        gameObject.SetActive(false);
    }

    void OnDestroy() {
        OverlaySystem.OnOverlayShown -= OnOverlayShown;
        OverlaySystem.OnOverlayHidden -= OnOverlayHidden;
    }

    void OnOverlayShown(OverlayData data) {
        if (data.CanPreview) {
            gameObject.SetActive(true);
        }
    }

    void OnOverlayHidden(OverlayData data) {
        gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData e) {
        ShowPreview();
    }

    public void OnPointerUp(PointerEventData e) {
        HidePreview();
    }

    static void HidePreview() {
        OverlaySystem.Instance.gameObject.SetActive(true);
    }

    static void ShowPreview() {
        OverlaySystem.Instance.gameObject.SetActive(false);
    }
}