using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class PhysicsTooltipTrigger : TooltipTrigger, IPointerMoveHandler {
    [SerializeField] Material _standardMaterial, _highlightMaterial;
    
    bool _active;
    bool _movedThisFrame;
    SpriteRenderer _spriteRenderer;

    protected override void Awake() {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerMove(PointerEventData e) {
        _movedThisFrame = true;
        if (_active) return;
        _active = true;

        _spriteRenderer.material = _highlightMaterial;
        Show();
    }

    void LateUpdate() {
        if (_active && !_movedThisFrame) {
            _active = false;
            _spriteRenderer.material = _standardMaterial;
            Hide();
        }
        _movedThisFrame = false;
    }
}