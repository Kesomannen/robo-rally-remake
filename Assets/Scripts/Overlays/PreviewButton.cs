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
        if (data._canPreview) {
            gameObject.SetActive(true);
        }
    }

    void OnOverlayHidden(OverlayData data) {
        gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData e) {
        if (e.button != PointerEventData.InputButton.Left) return;
        
        ShowPreview();
    }

    public void OnPointerUp(PointerEventData e) {
        if (e.button != PointerEventData.InputButton.Left) return;
        
        HidePreview();
    }

    static void HidePreview() {
        OverlaySystem.Instance.gameObject.SetActive(true);
    }

    static void ShowPreview() {
        OverlaySystem.Instance.gameObject.SetActive(false);
    }
}