using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMap : Singleton<UIMap>, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IPointerClickHandler {
    [Header("Animation")]
    [SerializeField] float _lerpTime;
    [Space]
    [SerializeField] float _focusedSize;
    [SerializeField] Vector2 _defaultPosition;
    [Space]
    [SerializeField] float _fullScreenSize;
    [SerializeField] Vector2 _fullScreenPosition;
    [Header("References")]
    [SerializeField] Camera _mapCamera;
    [SerializeField] Camera _uiCamera;
    [SerializeField] LeanTweenType _lerpType;

    public bool CanFocus { get; set; } = true;
    public bool InFullscreen { get; private set; }

    RectTransform _rectTransform;
    Physics2DRaycaster _raycaster;

    protected override void Awake() {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _raycaster = _mapCamera.GetComponent<Physics2DRaycaster>();
    }

    public void OnPointerEnter(PointerEventData e) {
        return;
        if (CanFocus) {
            LerpElementSize(_focusedSize);
        }
    }

    public void OnPointerExit(PointerEventData e) {
        return;
        if (CanFocus) {
            LerpElementSize(1);
        }
    }

    public void ZoomToFullscreen() {
        if (InFullscreen) return;
        InFullscreen = true;

        LerpElementSize(_fullScreenSize);
        LerpElementPosition(_fullScreenPosition);
    }

    public void ZoomToDefault() {
        if (!InFullscreen) return;
        InFullscreen = false;

        LerpElementSize(1);
        LerpElementPosition(_defaultPosition);
    }

    int _currentSizeTweenId;

    void LerpElementSize(float scale) {
        LeanTween.cancel(_currentSizeTweenId);
        _currentSizeTweenId = LeanTween.scale(gameObject, scale * Vector3.one, _lerpTime).setEase(_lerpType).id;
    }

    int _currentPosTweenId;

    void LerpElementPosition(Vector2 position) {
        LeanTween.cancel(_currentPosTweenId);
        _currentPosTweenId = LeanTween.move(_rectTransform, position, _lerpTime).setEase(_lerpType).id;
    }
    
    public void OnPointerMove(PointerEventData e) {
        Raycast(e, ExecuteEvents.moveHandler);
    }

    public void OnPointerClick(PointerEventData e) {
        Raycast(e, ExecuteEvents.pointerClickHandler);
    }

    void Raycast<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> func) where T : IEventSystemHandler {
        var t = transform;
        
        var mousePos = (Vector2) _uiCamera.ScreenToWorldPoint(eventData.position);
        var relative = mousePos - (Vector2)t.position;
        var rectSize = _rectTransform.rect.size * CanvasUtils.Scale / 2f;
        var viewPortPoint = relative / rectSize / 2 + Vector2.one * 0.5f;

        eventData.position = _mapCamera.ViewportToScreenPoint(viewPortPoint);
        var results = new List<RaycastResult>();
        _raycaster.Raycast(eventData, results);
        results.ForEach(r => ExecuteEvents.Execute(r.gameObject, eventData, func));
    }
}