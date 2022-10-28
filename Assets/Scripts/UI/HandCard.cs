using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCard : ProgramCard, IDragHandler, IBeginDragHandler, IEndDragHandler {
    static Canvas canvas;
    static GraphicRaycaster graphicRaycaster;

    Player _owner => NetworkSystem.LocalPlayer;

    void Awake() {
        if (canvas == null) {
            canvas = FindObjectOfType<Canvas>();
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }
    }

    public void OnBeginDrag(PointerEventData e) {
        Drag(e);
    }

    public void OnDrag(PointerEventData e) {
        Drag(e);
    }

    public void OnEndDrag(PointerEventData e) {
        var results = new List<RaycastResult>();
        graphicRaycaster.Raycast(e, results);

        foreach (var hit in results) {
            if (hit.gameObject.TryGetComponent(out Register register)) {
                if (register.SetCard(Data)) {
                    _owner.Hand.RemoveCard(Data);
                    return;
                }
            }
        }
    }

    void Drag(PointerEventData e) {
        transform.localPosition += new Vector3(e.delta.x, e.delta.y, 0) / transform.lossyScale.x;
    }
}