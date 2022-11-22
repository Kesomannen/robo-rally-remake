using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMap : Singleton<UIMap>, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler {
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
    Vector2 _lastMousePos;

    protected override void Awake() {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _raycaster = _mapCamera.GetComponent<Physics2DRaycaster>();
    }

    public void OnPointerEnter(PointerEventData e) {
        if (CanFocus) {
            LerpElementSize(_focusedSize);
        }
    }

    public void OnPointerExit(PointerEventData e) {
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
        if (_lastMousePos == e.position) return;
        Raycast(e, ExecuteEvents.pointerMoveHandler);
    }

    readonly List<RaycastResult> _raycastResults = new();
    
    void Raycast<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> func) where T : IEventSystemHandler {
        var t = transform;
        
        var mousePos = (Vector2) _uiCamera.ScreenToWorldPoint(eventData.position);

        var rectSize = _rectTransform.rect.size * t.localScale * CanvasUtils.Scale / 2f;
        var pivot = _rectTransform.pivot - new Vector2(0.5f, 0.5f);
        var rectPos = (Vector2) t.position - pivot * rectSize * 2f;

        var relative = mousePos - rectPos;
        var viewPortPoint = relative / rectSize / 2f + Vector2.one * 0.5f;

        eventData.position = _mapCamera.ViewportToScreenPoint(viewPortPoint);
        //_debugPos = _mapCamera.ViewportToWorldPoint(viewPortPoint);
        
        _raycastResults.Clear();
        _raycaster.Raycast(eventData, _raycastResults);
        switch (_raycastResults.Count){
            case 0: return;
            case > 1:
                ExecuteEvents.Execute(_raycastResults.OrderByDescending(r => r.sortingOrder).First().gameObject, eventData, func);
                break;
            default:
                ExecuteEvents.Execute(_raycastResults[0].gameObject, eventData, func);
                break;
        }
    }
    
    /*
    Vector2 _debugPos;
    
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_debugPos, 0.3f);
    }
    */
}