using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCard : ProgramCard, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [Header("Animation")]
    [SerializeField] float _highlightJumpHeight;
    [SerializeField] float _highlightedSize;
    [SerializeField] LeanTweenType _easingType;
    
    static CanvasScaler _canvasScaler;
    static GraphicRaycaster _graphicRaycaster;

    Player _owner => PlayerManager.LocalPlayer;
    bool _isDragging;
    int _index;
    Vector3 _origin;
    Transform _originalParent;

    bool IsHighlited {
        get => _isHighlighted;
        set {
            if (_isHighlighted == value) return;
            _isHighlighted = value;

            if (_isHighlighted) {
                transform.SetParent(_canvasScaler.transform);
                transform.SetAsLastSibling();

                var targetHeight = _canvasScaler.scaleFactor * _highlightJumpHeight;
                LerpTo(transform.position + Vector3.up * targetHeight);
                LeanTween.scale(gameObject, Vector3.one * _highlightedSize, 0.2f).setEase(_easingType);
            } else {
                transform.SetParent(_originalParent);
                transform.SetSiblingIndex(_index);

                LerpTo(_origin);
                LeanTween.scale(gameObject, Vector3.one, 0.2f).setEase(_easingType);
            }
        }
    }

    bool _isHighlighted;

    void Awake() {
        _originalParent = transform.parent;
        if (_canvasScaler == null) {
            _canvasScaler = FindObjectOfType<CanvasScaler>();
            _graphicRaycaster = _canvasScaler.GetComponent<GraphicRaycaster>();
        }
    }

    public void SetOrigin(Vector2 origin, int index) {
        _origin = origin;
        LerpTo(origin);
        _index = index;
        transform.SetSiblingIndex(_index);
    }

    public void OnPointerEnter(PointerEventData e) {
        IsHighlited = true;
    }

    public void OnPointerExit(PointerEventData e) {
        IsHighlited = false;
    }

    public void OnPointerClick(PointerEventData e) {
        if (_isDragging) return;

        for (int i = 0; i < ExecutionPhase.RegisterCount; i++) {
            var registerCard = _owner.Registers[i];
            if (registerCard == null) {
                if (TryPlace(RegisterUI.GetRegister(i))) {
                    return;
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData e) {
        if (e.button != PointerEventData.InputButton.Left) return;

        _isDragging = true;
        Drag(e);
    }

    public void OnDrag(PointerEventData e) {
        if (!_isDragging) return;

        Drag(e);
    }

    public void OnEndDrag(PointerEventData e) {
        if (!_isDragging) return;

        _isDragging = false;
        IsHighlited = false;

        var results = new List<RaycastResult>();
        _graphicRaycaster.Raycast(e, results);

        foreach (var hit in results) {
            if (hit.gameObject.TryGetComponent(out RegisterUI register)) {
                TryPlace(register);
                return;
            }
        }

        // failed to place, return to original position
        LerpTo(_origin);
    }

    bool TryPlace(RegisterUI register) {
        if (register.Place(this)) {
            _owner.Hand.RemoveCard(_index);
            return true;
        }
        return false;
    }

    void Drag(PointerEventData e) {
        transform.position += (Vector3)e.delta;
    }

    int _currentMoveTweenId;

    void LerpTo(Vector2 target, Action callback = null) {
        LeanTween.cancel(_currentMoveTweenId);
        _currentMoveTweenId = LeanTween
            .move(gameObject, target, 0.2f)
            .setEase(_easingType)
            .setOnComplete(() => {
                callback?.Invoke();
            }).id;
    }
}