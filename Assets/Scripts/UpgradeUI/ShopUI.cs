using System.Collections;
using System.Linq;
using UnityEngine;

public class ShopUI : MonoBehaviour {
    [Header("References")]
    [SerializeField] Transform _upgradeParent;
    [SerializeField] OverlayData<Choice<UpgradeCardData>> _overrideOverlay;
    
    [Header("Prefabs")]
    [SerializeField] ShopCard _shopCardPrefab;

    ShopCard[] _shopCards;

    void Awake() {
        _shopCards = new ShopCard[GameSettings.Instance.ShopSlots];
        gameObject.SetActive(false);
        ShopPhase.OnRestock += OnRestock;
        ShopPhase.OnPlayerDecision += OnPlayerDecision;
    }

    void OnDestroy() {
        ShopPhase.OnRestock -= OnRestock;
        ShopPhase.OnPlayerDecision -= OnPlayerDecision;
    }

    void OnRestock(int index, UpgradeCardData card) {
        var shopCard = _shopCards[index];
        if (shopCard != null) {
            shopCard.OnCardClicked -= OnCardClicked;
            shopCard.Remove();
        }
        var newCard = Instantiate(_shopCardPrefab, _upgradeParent);
        newCard.transform.SetSiblingIndex(index);
        _shopCards[index] = newCard;
        
        newCard.SetContent(card);
        newCard.OnCardClicked += OnCardClicked;
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
                result,
                5.0f);
            ShopPhase.Instance.MakeDecision(false, card, result.Index);
        }
    }
    
    void OnPlayerDecision(Player player, bool skipped, UpgradeCardData card) {
        if (skipped) return;
        for (var i = 0; i < _shopCards.Length; i++){
            var shopCard = _shopCards[i];
            if (shopCard == null || shopCard.Content != card) continue;

            shopCard.OnBuy();
            _shopCards[i] = null;
            break;
        }
    }

    public void Skip() {
        ShopPhase.Instance.MakeDecision(true, null, -1);
    }
}