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

    static GraphicRaycaster _graphicRaycaster;

    Player Owner => PlayerManager.LocalPlayer;
    bool _isDragging;
    int _index;
    Vector3 _origin;
    Transform _originalParent;

    private void SetHighlighted(bool value) {
        if (_isHighlighted == value) return;
        _isHighlighted = value;

        if (_isHighlighted) {
            transform.SetParent(_graphicRaycaster.transform);
            transform.SetAsLastSibling();

            var targetHeight = CanvasUtils.Scale.y * _highlightJumpHeight;
            LerpTo(transform.position + Vector3.up * targetHeight);
            LeanTween.scale(gameObject, Vector3.one * _highlightedSize, 0.2f).setEase(_easingType);
        } else {
            transform.SetParent(_originalParent);
            transform.SetSiblingIndex(_index);

            LerpTo(_origin);
            LeanTween.scale(gameObject, Vector3.one, 0.2f).setEase(_easingType);
        }
    }

    bool _isHighlighted;

    void Awake() {
        _originalParent = transform.parent;
        if (_graphicRaycaster == null) {
            _graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        }
    }

    void OnEnable() {
        transform.position = _origin;    
    }

    public void SetOrigin(Vector2 origin, int index) {
        _origin = origin;
        _index = index;
        transform.SetSiblingIndex(_index);

        if (enabled) LerpTo(origin);
    }

    public void OnPointerEnter(PointerEventData e) {
        SetHighlighted(true);
    }

    public void OnPointerExit(PointerEventData e) {
        SetHighlighted(false);
    }

    public void OnPointerClick(PointerEventData e) {
        if (_isDragging) return;

        for (int i = 0; i < ExecutionPhase.RegisterCount; i++) {
            var register = RegisterUI.GetRegisterUI(i);
            if (register.IsEmpty && TryPlace(register)) {
                break;
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
        SetHighlighted(false);

        var results = new List<RaycastResult>();
        _graphicRaycaster.Raycast(e, results);

        foreach (var hit in results) {
            if (hit.gameObject.TryGetComponent(out RegisterUI register)) {
                TryPlace(register);
                return;
            }
        }

        LerpTo(_origin);
    }

    bool TryPlace(RegisterUI register) {
        if (register.Place(this)) {
            Owner.Hand.RemoveCard(_index);
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