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
    static Player Owner => PlayerManager.LocalPlayer;
    
    bool _isDragging;
    int _index;
    Vector3 _origin;
    Transform _originalParent;
    RectTransform _rectTransform;

    void SetHighlighted(bool value) {
        if (_isHighlighted == value) return;
        _isHighlighted = value;

        if (_isHighlighted){
            var t = transform;
            t.SetParent(_graphicRaycaster.transform);
            t.SetAsLastSibling();

            var targetHeight = CanvasUtils.CanvasScale.y * _highlightJumpHeight;
            LerpTo(t.position + Vector3.up * targetHeight);
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
        _rectTransform = GetComponent<RectTransform>();
        
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
        _isDragging = false;
    }

    public void OnPointerClick(PointerEventData e) {
        if (_isDragging) return;

        for (var i = 0; i < ExecutionPhase.RegisterCount; i++) {
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

        foreach (var hit in results){
            if (!hit.gameObject.TryGetComponent(out RegisterUI register)) continue;
            TryPlace(register);
            return;
        }

        LerpTo(_origin);
    }

    bool TryPlace(RegisterUI register){
        if (!register.Place(this)) return false;
        Owner.Hand.RemoveCard(_index);
        return true;
    }

    void Drag(PointerEventData e) {
        _rectTransform.anchoredPosition += e.delta / CanvasUtils.ScreenScale;
    }

    int _currentMoveTweenId;

    void LerpTo(Vector2 target, Action callback = null) {
        LeanTween.cancel(_currentMoveTweenId);
        _currentMoveTweenId = LeanTween
            .move(gameObject, new Vector3(target.x, target.y), 0.2f)
            .setEase(_easingType)
            .setOnComplete(() => {
                callback?.Invoke();
            }).id;
    }
}