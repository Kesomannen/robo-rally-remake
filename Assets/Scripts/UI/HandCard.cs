using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCard : ProgramCard, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [SerializeField] Image _backgroundImage;
    [SerializeField] Sprite _standardSprite, _highlightedSprite;
    [SerializeField] float _highlightJumpHeight;
    [SerializeField] LeanTweenType _easingType;
    [SerializeField] float _highlightedSize;
    
    static Canvas _canvas;
    static GraphicRaycaster _graphicRaycaster;

    Player _owner => NetworkSystem.LocalPlayer;

    bool _canDrag = true;
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
                _backgroundImage.sprite = _highlightedSprite;
                transform.SetParent(_canvas.transform);
                transform.SetAsLastSibling();

                LerpTo(transform.position + Vector3.up * _highlightJumpHeight);
                LeanTween.scale(gameObject, Vector3.one * _highlightedSize, 0.2f).setEase(_easingType);
            } else {
                _backgroundImage.sprite = _standardSprite;
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
        if (_canvas == null) {
            _canvas = FindObjectOfType<Canvas>();
            _graphicRaycaster = _canvas.GetComponent<GraphicRaycaster>();
        }
    }

    public void SetOrigin(Vector2 origin, int index) {
        _origin = origin;
        LerpTo(origin);
        _index = index;
        transform.SetSiblingIndex(_index);
    }

    public void OnPointerEnter(PointerEventData e) => IsHighlited = true;
    public void OnPointerExit(PointerEventData e) => IsHighlited = false;

    public void OnPointerClick(PointerEventData e) {
        if (_isDragging) return;

        var firstOpenRegister = _owner.Registers.FirstOrDefault(r => r.IsEmpty);
        if (firstOpenRegister != null) {
            TryPlace(firstOpenRegister);
        }
    }

    public void OnBeginDrag(PointerEventData e) {
        if (!_canDrag || e.button != PointerEventData.InputButton.Left) return;

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
            if (hit.gameObject.TryGetComponent(out Register register)) {
                TryPlace(register);
                return;
            }
        }

        // failed to place, return to original position
        LerpTo(_origin);
    }

    void TryPlace(Register register) {
        if (register.Place(this)) {
            _owner.Hand.RemoveCard(_index);
        }
    }

    void Drag(PointerEventData e) {
        transform.position += (Vector3)e.delta;
    }

    LTDescr _currentTween;

    void LerpTo(Vector2 target, Action callback = null) {
        if (_currentTween != null) LeanTween.cancel(_currentTween.id);
        _currentTween = LeanTween
            .move(gameObject, target, 0.2f)
            .setEase(_easingType)
            .setOnComplete(() => {
                _currentTween = null;
                callback?.Invoke();
            });
    }
}