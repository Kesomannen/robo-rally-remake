using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class ShopUIController : Singleton<ShopUIController> {
    [Header("References")]
    [SerializeField] TMP_Text _currentPlayerText;
    [SerializeField] Transform _upgradeParent;
    [SerializeField] PlayerPanelArray _panelArray;
    [SerializeField] OverlayData<Choice<UpgradeCardData>> _overrideOverlay;
    
    [Header("Prefabs")]
    [SerializeField] ShopCard _shopCardPrefab;

    ShopCard[] _shopCards;
    
    public OverlayData<Choice<UpgradeCardData>> OverrideOverlay => _overrideOverlay;

    protected override void Awake() {
        base.Awake();
        
        _shopCards = new ShopCard[LobbySystem.LobbySettings.ShopCards];
        for (var i = 0; i < _shopCards.Length; i++) {
            var card = Instantiate(_shopCardPrefab, _upgradeParent);
            card.OnCardClicked += OnCardClicked;
            _shopCards[i] = card;
        }
        
        gameObject.SetActive(false);
        ShopPhase.OnRestock += OnRestock;
        ShopPhase.OnPlayerDecision += OnPlayerDecision;
        ShopPhase.OnNewPlayer += OnNewPlayer;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        
        ShopPhase.OnRestock -= OnRestock;
        ShopPhase.OnPlayerDecision -= OnPlayerDecision;
        ShopPhase.OnNewPlayer -= OnNewPlayer;
    }
    
    void OnNewPlayer(Player player) {
        if (player == null) {
            _currentPlayerText.text = "All players skipped, restocking...";
            return;
        }
        _currentPlayerText.text = $"{player} is buying...";
        UpdateCards();
    }

    void OnRestock(int index, UpgradeCardData card) {
        var shopCard = _shopCards[index];
        TaskScheduler.PushRoutine(shopCard.RestockAnimation(card));
        UpdateCards();
    }
    
    void OnCardClicked(ShopCard shopCard) {
        var card = shopCard.Content;
        var localPlayer = PlayerSystem.LocalPlayer;
        if (!shopCard.Available) return;

        for (var i = 0; i < localPlayer.Upgrades.Count; i++) {
            if (localPlayer.Upgrades[i] != null) continue;
            ShopPhase.Instance.MakeDecision(false, card, i);
            return;
        }
        StartCoroutine(ChoseOverride());

        IEnumerator ChoseOverride() {
            var overlay = OverlaySystem.Instance.PushAndShowOverlay(_overrideOverlay);
            overlay.Init(localPlayer.Upgrades, true);
            yield return new WaitUntil(() => overlay.IsDone);

            if (!overlay.WasCancelled) {
                ShopPhase.Instance.MakeDecision(false, card, overlay.SelectedOptions[0]);
            }
        }
    }
    
    void OnPlayerDecision(Player player, bool skipped, UpgradeCardData card) {
        if (skipped) {
            
        } else {
            TaskScheduler.PushRoutine(_shopCards.First(c => c.Content == card)
                .BuyAnimation(_panelArray.Panels.First(p => p.Content == player).transform));
        }
    }
    
    public void Skip() {
        ShopPhase.Instance.MakeDecision(true, null, -1);
    }

    void UpdateCards() {
        foreach (var card in _shopCards) {
            card.UpdateAvailability();
        }
    }
}