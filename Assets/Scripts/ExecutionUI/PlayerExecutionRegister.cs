using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerExecutionRegister : MonoBehaviour, ITooltipable {
    [Header("References")]
    [SerializeField] Image _programCardImage;
    [SerializeField] Image _backgroundImage;
    [SerializeField] Selectable _selectable;

    [Header("Animation")]
    [SerializeField] float _tweenTime;
    [SerializeField] LeanTweenType _tweenType;
    
    [Header("Sprites")]
    [FormerlySerializedAs("_hiddenUnselected")]
    [SerializeField] Sprite _sprite0;
    [FormerlySerializedAs("_hiddenSelected")]
    [SerializeField] Sprite _sprite10;
    [FormerlySerializedAs("_visibleUnselected")] 
    [SerializeField] Sprite _sprite1;
    [FormerlySerializedAs("_visibleSelected")] 
    [SerializeField] Sprite _sprite11;
    [SerializeField] Sprite _emptySprite;

    public string Header => _visible ? (_content == null ? "Empty" : _content.Header) : "???";
    public string Description => _visible ? _content == null ? "" : _content.Description : "???";

    RectTransform _rectTransform;
    RectTransform _programCardTransform;
    ProgramCardData _content;
    bool _visible;

    public bool Visible {
        get => _visible;
        set {
            _visible = value;
            _programCardImage.enabled = _visible;
            _backgroundImage.sprite = _visible ? _sprite0 : _sprite1;
            
            var spriteState = _selectable.spriteState;
            spriteState.highlightedSprite = _visible ? _sprite10 : _sprite11;
            _selectable.spriteState = spriteState;
        }
    }

    Color _color;

    public Color Color {
        get => _color;
        set {
            if (value == _color) return;
            _color = value;

            _backgroundImage.color = _color;
            _programCardImage.color = _color;
        }
    }

    Vector2 _baseSize;
    Vector2 _programCardBaseSize;
    float _scale = 1;
    int _tweenId;

    public float Scale {
        get => _scale;
        set {
            if (Math.Abs(_scale - value) < 0.05f) return;
            var prev = _scale;

            LeanTween.cancel(_tweenId);
            _tweenId = TweenHelper.TweenValue(prev, value, _tweenTime, _tweenType, v => {
                _scale = v;
                _rectTransform.sizeDelta = v * _baseSize;
                _programCardTransform.sizeDelta = v * _programCardBaseSize;
            });
        }
    }

    void Awake() {
        _rectTransform = GetComponent<RectTransform>();
        _programCardTransform = _programCardImage.GetComponent<RectTransform>();
        _baseSize = _rectTransform.sizeDelta;
        _programCardBaseSize = _programCardTransform.sizeDelta;
        Visible = false;
    }

    public void SetContent(ProgramCardData card) {
        _content = card;
        _programCardImage.sprite = card == null ? _emptySprite : card.Artwork;
    }
}