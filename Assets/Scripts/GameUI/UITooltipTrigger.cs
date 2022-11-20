using UnityEngine.EventSystems;

public class UITooltipTrigger : TooltipTrigger, IPointerEnterHandler, IPointerExitHandler {
    public void OnPointerEnter(PointerEventData e) => Show();
    public void OnPointerExit(PointerEventData e) => Hide();
}