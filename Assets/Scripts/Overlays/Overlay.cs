using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Overlay : MonoBehaviour {
    protected virtual void OnEnable() {
        OverlaySystem.OnClick += OnOverlayClick;
    }

    protected virtual void OnDisable() {
        OverlaySystem.OnClick -= OnOverlayClick;
    }

    protected virtual void OnOverlayClick() {
        OverlaySystem.Instance.DestroyCurrentOverlay();
    }
}