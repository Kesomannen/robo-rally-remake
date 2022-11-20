using UnityEngine;
using UnityEngine.EventSystems;

public class PhysicsTooltipTrigger : TooltipTrigger, IPointerMoveHandler {
    bool _active;
    
    public void OnPointerMove(PointerEventData e) {
        Debug.Log("OnPointerMove");
        if (!_active) return;
        _active = true;

        Show();
    }
}