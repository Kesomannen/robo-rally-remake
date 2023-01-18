using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Highlight))]
public class PhysicsTooltipTrigger : TooltipTrigger, IPointerMoveHandler {
    bool _active;
    bool _movedThisFrame;
    Highlight _highlight;

    protected override void Awake() {
        base.Awake();
        _highlight = GetComponent<Highlight>();
        _highlight.enabled = false;
    }

    public void OnPointerMove(PointerEventData e) {
        _movedThisFrame = true;
        if (_active) return;
        _active = true;

        _highlight.enabled = true;
        Show();
    }

    void Update() {
        if (_active && !_movedThisFrame) {
            _active = false;
            
            _highlight.enabled = false;
            Hide();
        }
        _movedThisFrame = false;
    }
}