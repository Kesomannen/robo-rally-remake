using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Container<>))]
public class ChoiceItem<T> : MonoBehaviour, IPointerClickHandler {
    [Header("References")]
    [SerializeField] Selectable _selectable;
    [SerializeField] GameObject _unavailableOverlay;
    [SerializeField] Container<T> _container;
    [SerializeField] [ReadOnly] int _optionIndex;

    [Header("Animation")]
    [SerializeField] float _selectedSize;
    [SerializeField] float _selectDuration;
    [SerializeField] LeanTweenType _selectTweenType;

    Sprite _selectedSprite;
    Sprite _defaultSprite;
    bool _selected;
    bool _isAvailable;
    int _scaleTweenId;
    
    public int OptionIndex {
        get => _optionIndex;
        set => _optionIndex = value;
    }

    public Container<T> Container => _container;

    public static event Action<int> OnItemClicked;

    void Awake() {
        _selectedSprite = _selectable.spriteState.highlightedSprite;
        _defaultSprite = _selectable.image.sprite;
    }

    public void OnPointerClick(PointerEventData e) {
        if (!_isAvailable) return;
        OnItemClicked?.Invoke(OptionIndex);
        SetSelected(!_selected);
    }

    void SetSelected(bool selected) {
        _selected = selected;
        
        LeanTween.cancel(_scaleTweenId);
        _scaleTweenId = LeanTween
            .scale(gameObject, Vector3.one * (_selected ? _selectedSize : 1), _selectDuration)
            .setEase(_selectTweenType)
            .uniqueId;
        _selectable.image.sprite = selected ? _selectedSprite : _defaultSprite;
    }
    
    public void SetAvailable(bool available) {
        _isAvailable = available;
        _selectable.interactable = available;
        _unavailableOverlay.SetActive(!available);
    }
}