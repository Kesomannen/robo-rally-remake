using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandProgramCard : ProgramCard, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [Header("Animation")]
    [SerializeField] float _jumpHeight;
    [SerializeField] float _jumpDuration;
    [SerializeField] float _highlightedSize;
    [SerializeField] LeanTweenType _easingType;
    [SerializeField] float _moveDuration;

    static GraphicRaycaster _graphicRaycaster;
    static Player Owner => PlayerSystem.LocalPlayer;
    public Transform HighlightParent { set => _highlightParent = value; }
    
    bool _isDragging;
    int _index;
    Vector3 _origin;
    float _verticalOffset;
    Transform _originalParent;
    Transform _highlightParent;
    RectTransform _rectTransform;
    int _scaleTweenId;
    
    bool IsProgramming => PhaseSystem.Current.Value == Phase.Programming;

    void SetHighlighted(bool value) {
        if (_isHighlighted == value) return;
        _isHighlighted = value;

        if (_isHighlighted) {
            var t = transform;
            transform.SetParent(_highlightParent);

            var targetHeight = CanvasUtils.CanvasScale.y * (_jumpHeight - _verticalOffset);
            LerpTo(t.position + Vector3.up * targetHeight, _jumpDuration);
            
            LeanTween.cancel(_scaleTweenId);
            _scaleTweenId = LeanTween
                .scale(gameObject, Vector3.one * _highlightedSize, _jumpDuration)
                .setEase(_easingType)
                .uniqueId;
        } else {
            var t = transform;
            t.SetParent(_originalParent);
            t.SetSiblingIndex(_index);

            LerpTo(_origin, _jumpDuration);
            
            LeanTween.cancel(_scaleTweenId);
            _scaleTweenId = LeanTween
                .scale(gameObject, Vector3.one, _jumpDuration)
                .setEase(_easingType)
                .uniqueId;
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

    public void SetOrigin(Vector2 origin, int index, float verticalOffset) {
        _origin = origin;
        _verticalOffset = verticalOffset;
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
        if (_isDragging || !IsProgramming) return;

        for (var i = 0; i < ExecutionPhase.RegisterCount; i++) {
            var register = PlayerRegisterUI.GetRegister(i);
            if (register.IsEmpty && TryPlace(register)) {
                break;
            }
        }
    }

    public void OnBeginDrag(PointerEventData e) {
        if (e.button != PointerEventData.InputButton.Left || !IsProgramming) return;

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
            if (!hit.gameObject.TryGetComponent(out PlayerRegisterUI register)) continue;
            TryPlace(register);
            return;
        }

        LerpTo(_origin);
    }

    bool TryPlace(PlayerRegisterUI playerRegisterUI){
        if (!playerRegisterUI.Place(this)) return false;
        Owner.Hand.RemoveCard(_index);
        return true;
    }

    void Drag(PointerEventData e) {
        _rectTransform.anchoredPosition += e.delta / CanvasUtils.ScreenScale;
    }

    int _currentMoveTweenId;

    void LerpTo(Vector2 target, float time = 0, Action callback = null) {
        var duration = time == 0 ? _moveDuration : time;
        LeanTween.cancel(_currentMoveTweenId);
        _currentMoveTweenId = LeanTween
            .move(gameObject, new Vector3(target.x, target.y), duration)
            .setEase(_easingType)
            .setOnComplete(() => {
                callback?.Invoke();
            }).uniqueId;
    }
}