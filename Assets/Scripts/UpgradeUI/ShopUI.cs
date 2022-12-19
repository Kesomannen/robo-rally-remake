using System;
using UnityEngine;

public class ShopUI : MonoBehaviour {
    [Header("References")]
    [SerializeField] Transform _upgradeParent;
    
    [Header("Prefabs")]
    [SerializeField] ShopCard _shopCardPrefab;

    ShopCard[] _shopCards;

    void Start() {
        _shopCards = new ShopCard[GameSettings.Instance.ShopSlots];
        gameObject.SetActive(false);
    }

    void OnEnable() {
        ShopPhase.OnRestock += OnRestock;
        ShopPhase.OnPlayerDecision += OnPlayerDecision;
    }

    void OnDisable() {
        ShopPhase.OnRestock -= OnRestock;
        ShopPhase.OnPlayerDecision -= OnPlayerDecision;
    }
    
    void OnRestock(int index, UpgradeCardData card) {
        var shopCard = _shopCards[index];
        if (shopCard != null){
            shopCard.OnCardClicked -= OnCardClicked;
            shopCard.Remove();
        }
        var newCard = Instantiate(_shopCardPrefab, _upgradeParent);
        _shopCards[index] = newCard;
        
        newCard.SetContent(card);
        newCard.OnCardClicked += OnCardClicked;
    }
    
    void OnCardClicked(ShopCard shopCard) {
        var localPlayer = PlayerManager.LocalPlayer;
        if (ShopPhase.CurrentPlayer != localPlayer) return;
        
        var card = shopCard.Content;
        if (card.Cost > localPlayer.Energy.Value) return;
        
        var slot = localPlayer.GetOpenUpgradeSlot();
        if (slot == -1) return;
        
        ShopPhase.Instance.MakeDecision(false, card, slot);
    }
    
    void OnPlayerDecision(Player player, bool skipped, UpgradeCardData card) {
        if (skipped) return;
        for (var i = 0; i < _shopCards.Length; i++){
            var shopCard = _shopCards[i];
            if (shopCard.Content != card) continue;
            
            shopCard.OnBuy();
            _shopCards[i] = null;
            break;
        }
    }

    public void Skip() {
        ShopPhase.Instance.MakeDecision(true, null, -1);
    }
}