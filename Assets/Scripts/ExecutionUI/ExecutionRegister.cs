using System;
using UnityEngine;
using UnityEngine.UI;

public class ExecutionRegister : Container<ProgramCardData>, ITooltipable {
    [Header("Execution Register")]
    [SerializeField] Sprite _closedSprite;
    [SerializeField] Sprite _closedSpriteHighlighted;
    [SerializeField] Sprite _openSprite;
    [SerializeField] Sprite _openSpriteHighlighted;
    
    [Header("References")]
    [SerializeField] Image _artworkImage;
    [SerializeField] Image _backgroundImage;
    [SerializeField] Selectable _selectable;

    bool _hidden;

    public string Header => _hidden ? "???" : Content.Header;
    public string Description => _hidden ? "???" : Content.Description;

    public bool Hidden {
        set {
            _hidden = value;
            _artworkImage.enabled = !_hidden;
            _backgroundImage.sprite = _hidden ? _closedSprite : _openSprite;
            
            var spriteState = _selectable.spriteState;
            spriteState.highlightedSprite = _hidden ? _closedSpriteHighlighted : _openSpriteHighlighted;
            _selectable.spriteState = spriteState;
        }
    }

    void OnEnable() {
        //Hidden = false;
    }

    protected override void Serialize(ProgramCardData data) {
        _artworkImage.sprite = data.Artwork;
    }
}