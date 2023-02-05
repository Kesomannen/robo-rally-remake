using System;
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
    [SerializeField] Image _unavailableOverlay;
    [SerializeField] Selectable _selectable;
    
    [Header("Sprites")]
    [SerializeField] Sprite[] _costSprites;
    [SerializeField] CardSprite _temporarySprite, _permanentSprite, _actionSprite;
    [SerializeField] bool _showOverlayOnClick = true;
    [SerializeField] UpgradeCardOverlay _overlay;

    [Serializable]
    struct CardSprite {
        public Sprite Default, Selected;
    }
    
    public string Header => $"{Content.Name} ({Content.Cost})";
    public string Description => Content.Description;
    
    public event Action<UpgradeCard> OnClick;

    void Awake() {
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerLockedIn;
        ProgrammingPhase.OnPhaseStarted += UpdateAvailability;
    }

    void OnDestroy() {
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerLockedIn;
        ProgrammingPhase.OnPhaseStarted += UpdateAvailability;
    }

    protected override void Serialize(UpgradeCardData card) {
        _nameText.text = card.Name;
        _artworkImage.sprite = card.Icon;
        _costImage.sprite = _costSprites[card.Cost];
        
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
        
        UpdateAvailability();
    }
    
    public void OnPointerClick(PointerEventData e) {
        if (Content == null) return;
        OnClick?.Invoke(this);

        if (!_showOverlayOnClick || e.button != PointerEventData.InputButton.Right) return;
        var overlay = new OverlayData<UpgradeCardOverlay> {
            _header = "Upgrade Card",
            _subtitle = Content.Name,
            _canPreview = false,
            _prefab = _overlay
        };
        OverlaySystem.Instance.PushAndShowOverlay(overlay).Init(Content);
    }

    void OnPlayerLockedIn(Player player) => UpdateAvailability();
    
    void UpdateAvailability() {
        var available = Content.CanUse(PlayerSystem.LocalPlayer);
        _unavailableOverlay.gameObject.SetActive(!available);
        _selectable.interactable = available;
    }
}