using UnityEngine;
using UnityEngine.UI;

public class PlayerExecutionRegister : Container<ProgramCardData>, ITooltipable {
    [Header("References")]
    [SerializeField] Image _programCardImage;
    [SerializeField] Image _backgroundImage;
    [SerializeField] Selectable _selectable;

    [Header("Sprites")]
    [SerializeField] Sprite _hiddenUnselected;
    [SerializeField] Sprite _hiddenSelected;
    [SerializeField] Sprite _visibleUnselected;
    [SerializeField] Sprite _visibleSelected;

    public string Header => _hidden ? "???" : Content.Header;
    public string Description => _hidden ? "???" : Content.Description;
    
    bool _hidden;

    public bool Hidden {
        get => _hidden;
        set {
            _hidden = value;
            _programCardImage.enabled = !_hidden;
            _backgroundImage.sprite = _hidden ? _hiddenUnselected : _visibleUnselected;
            
            var spriteState = _selectable.spriteState;
            spriteState.highlightedSprite = _hidden ? _hiddenSelected : _visibleSelected;
            _selectable.spriteState = spriteState;
        }
    }

    void Awake() {
        Hidden = false;
    }

    protected override void Serialize(ProgramCardData card) {
        _programCardImage.sprite = card.Artwork;
    }
}