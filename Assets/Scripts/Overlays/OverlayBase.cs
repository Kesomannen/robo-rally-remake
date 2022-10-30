using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class OverlayBase : MonoBehaviour {
    protected virtual void Awake() {
        OverlaySystem.OnClick += OnOverlayClick;
    }

    protected virtual void OnOverlayClick(PointerEventData e) {
        OverlaySystem.Instance.HideOverlay();
    }
}