using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class ShopUI : MonoBehaviour {
    [Header("References")]
    [SerializeField] TMP_Text _currentPlayerText;
    [SerializeField] Transform _upgradeParent;
    [SerializeField] OverlayData<Choice<UpgradeCardData>> _overrideOverlay;
    
    [Header("Prefabs")]
    [SerializeField] ShopCard _shopCardPrefab;

    ShopCard[] _shopCards;

    void Awake() {
        _shopCards = new ShopCard[GameSettings.Instance.ShopSlots];
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

    void OnDestroy() {
        ShopPhase.OnRestock -= OnRestock;
        ShopPhase.OnPlayerDecision -= OnPlayerDecision;
        ShopPhase.OnNewPlayer -= OnNewPlayer;
    }
    
    void OnNewPlayer(Player player) {
        _currentPlayerText.text = $"{player} is buying...";
    }

    void OnRestock(int index, UpgradeCardData card) {
        var shopCard = _shopCards[index]; 
        shopCard.SetContent(card);
        TaskScheduler.PushRoutine(shopCard.RestockAnimation());
    }
    
    void OnCardClicked(ShopCard shopCard) {
        var localPlayer = PlayerSystem.LocalPlayer;
        if (ShopPhase.CurrentPlayer != localPlayer) return;
        
        var card = shopCard.Content;
        if (card.Cost > localPlayer.Energy.Value) return;
        
        for (var i = 0; i < localPlayer.Upgrades.Count; i++) {
            if (localPlayer.Upgrades[i] != null) continue;
            ShopPhase.Instance.MakeDecision(false, card, i);
            return;
        }
        StartCoroutine(ChoseOverride());

        IEnumerator ChoseOverride() {
            var result = new Choice<UpgradeCardData>.ChoiceResult();
            yield return Choice<UpgradeCardData>.Create(
                localPlayer,
                _overrideOverlay,
                localPlayer.Upgrades,
                Enumerable.Repeat(true, localPlayer.Upgrades.Count).ToArray(),
                result);
            ShopPhase.Instance.MakeDecision(false, card, result.Index);
        }
    }
    
    void OnPlayerDecision(Player player, bool skipped, UpgradeCardData card) {
        if (skipped) {
            
        } else {
            TaskScheduler.PushRoutine(_shopCards.First(c => c.Content == card).BuyAnimation());
        }
    }

    // Used in button UnityEvent
    public void Skip() {
        ShopPhase.Instance.MakeDecision(true, null, -1);
    }
}