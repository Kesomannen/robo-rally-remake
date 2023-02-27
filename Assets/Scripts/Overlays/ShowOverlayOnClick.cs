using UnityEngine;
using UnityEngine.EventSystems;

public class ShowOverlayOnClick : MonoBehaviour, IPointerClickHandler {
    [SerializeField] OverlayData<Overlay> _overlayData;
    [SerializeField] bool _showOnRightClick;
    [SerializeField] bool _showOnLeftClick = true;

    public void OnPointerClick(PointerEventData eventData) {
        if (!enabled) return;

        if (eventData.button == PointerEventData.InputButton.Right && !_showOnRightClick
            || eventData.button == PointerEventData.InputButton.Left && !_showOnLeftClick) {
            return;
        }
        OverlaySystem.Instance.PushAndShowOverlay(_overlayData);

    }
}