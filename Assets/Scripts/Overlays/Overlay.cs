using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Overlay : MonoBehaviour {
    protected virtual void Awake() {
        OverlaySystem.OnClick += OnOverlayClick;
    }

    protected virtual void OnDestroy() {
        OverlaySystem.OnClick -= OnOverlayClick;
    }

    protected virtual void OnOverlayClick() {
        OverlaySystem.Instance.HideOverlay();
    }
}