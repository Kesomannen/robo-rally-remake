using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RobotPanel : Container<RobotData>, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [SerializeField] Image _iconImage;
    [SerializeField] Image _backgroundImage;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _descriptionText;
    [SerializeField] UISounds _uiSounds;
    [Space]
    [SerializeField] Sprite _defaultSprite;
    [SerializeField] Sprite _highlightedSprite;
    [SerializeField] Sprite _selectedSprite;
    [Space]
    [SerializeField] Color _grayColor, _unavailableColor;
    [SerializeField] float _hoverScale;
    [SerializeField] float _tweenDuration;
    [SerializeField] LeanTweenType _tweenType;

    State _state;
    
    public enum State {
        Available,
        Unavailable,
        Selected
    }

    public void SetState(State state) {
        if (state == _state) return;
        _state = state;
        
        Debug.Log($"Set state to {_state}", this);
        
        _uiSounds.enabled = _state == State.Available;

        switch (_state) {
            case State.Available:
                _backgroundImage.sprite = _defaultSprite;
                FadeIconTo(_grayColor);
                ScaleElementsTo(1);
                break;
            
            case State.Unavailable:
                _backgroundImage.sprite = _defaultSprite;
                FadeIconTo(_unavailableColor);
                ScaleElementsTo(0.8f);
                break;
            
            case State.Selected:
                _backgroundImage.sprite = _selectedSprite;
                FadeIconTo(Color.white);
                ScaleElementsTo(_hoverScale);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void Awake() {
        _scaleTargets = new[] {
            _iconImage.gameObject, _nameText.gameObject, _descriptionText.gameObject
        };
        _scaleTweenIds = new int[_scaleTargets.Length];
    }

    public void OnPointerEnter(PointerEventData e) {
        if (_state != State.Available) return;
        _backgroundImage.sprite = _highlightedSprite;
        ScaleElementsTo(_hoverScale);
        FadeIconTo(Color.white);
    }

    public void OnPointerExit(PointerEventData e) {
        if (_state != State.Available) return;
        _backgroundImage.sprite = _defaultSprite;
        ScaleElementsTo(1);
        FadeIconTo(_grayColor);
    }
    
    public void OnPointerClick(PointerEventData e) {
        if (_state != State.Available) return;
        LobbySystem.Instance.UpdatePlayer(robot: Content);
    }
    
    int[] _scaleTweenIds;
    GameObject[] _scaleTargets;

    void ScaleElementsTo(float scale) {
        foreach (var id in _scaleTweenIds) {
            LeanTween.cancel(id);
        }

        for (var i = 0; i < _scaleTargets.Length; i++) {
            var target = _scaleTargets[i];
            _scaleTweenIds[i] = LeanTween
                .scale(target, Vector3.one * scale, _tweenDuration)
                .setEase(_tweenType)
                .uniqueId;
        }
    }

    int _fadeTweenId;

    void FadeIconTo(Color color) {
        LeanTween.cancel(_fadeTweenId);
        _fadeTweenId = LeanTween
            .value(_iconImage.gameObject, _iconImage.color, color, _tweenDuration)
            .setEase(_tweenType)
            .setOnUpdate(c => _iconImage.color = c)
            .uniqueId;
    }

    protected override void Serialize(RobotData data) {
        _iconImage.sprite = data.Icon;
        _nameText.text = data.Name;
        _descriptionText.text = data.Description;
    }
}