﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCard : Container<UpgradeCardData>, IPointerClickHandler, ITooltipable {
    [Header("References")]
    [SerializeField] TMP_Text _nameText;
    [SerializeField] Image _costImage;
    [SerializeField] Image _artworkImage;
    [SerializeField] Image _backgroundImage;
    [SerializeField] Selectable _selectable;
    
    [Header("Sprites")]
    [SerializeField] Sprite[] _costSprites;
    [SerializeField] CardSprite _temporarySprite, _permanentSprite, _actionSprite;
    [SerializeField] UpgradeCardOverlay _overlay;

    [Serializable]
    struct CardSprite {
        public Sprite Default, Selected;
    }
    
    public string Header => $"{Data.Name} ({Data.Cost})";
    public string Description => Data.Description;

    protected override void Serialize(UpgradeCardData card) {
        _nameText.text = card.Name;
        _artworkImage.sprite = card.Icon;
        _costImage.sprite = _costSprites[card.Cost - 1];
        
        var cardSprite = card.Type switch {
            UpgradeType.Temporary => _temporarySprite,
            UpgradeType.Permanent => _permanentSprite,
            UpgradeType.Action => _actionSprite,
            _ => throw new ArgumentOutOfRangeException()
        };
        _backgroundImage.sprite = cardSprite.Default;

        var spriteState = _selectable.spriteState;
        spriteState.highlightedSprite = cardSprite.Selected;
        spriteState.pressedSprite = cardSprite.Selected;
        spriteState.selectedSprite = cardSprite.Selected;
        _selectable.spriteState = spriteState;
    }
    
    public void OnPointerClick(PointerEventData e) {
        if (Data == null) return;
        if (e.button == PointerEventData.InputButton.Right){
            var overlay = new OverlayData<UpgradeCardOverlay> {
                Header = "Upgrade Card",
                Subtitle = Data.Name,
                CanPreview = false,
                Prefab = _overlay
            };
            OverlaySystem.Instance.ShowOverlay(overlay)?.Init(Data);
        }
    }
}