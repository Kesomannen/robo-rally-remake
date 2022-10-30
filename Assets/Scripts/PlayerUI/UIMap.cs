using UnityEngine;
using UnityEngine.EventSystems;

public class UIMap : Singleton<UIMap>, IPointerEnterHandler, IPointerExitHandler {
    [Header("Animation")]
    [SerializeField] float _lerpTime;
    [Space]
    [SerializeField] float _focusedSize;
    [SerializeField] Vector2 _defaultPosition;
    [Space]
    [SerializeField] float _fullScreenSize;
    [SerializeField] Vector2 _fullScreenPosition;
    [Header("References")]
    [SerializeField] Camera _camera;
    [SerializeField] LeanTweenType _lerpType;

    public bool CanFocus { get; set; } = true;
    public bool InFullscreen { get; private set; }

    RectTransform _rectTransform;

    protected override void Awake() {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
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
}