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
    }
    void OnDisable() {
        ShopPhase.OnRestock -= OnRestock;
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
        
        var card = shopCard.Data;
        if (card.Cost > localPlayer.Energy.Value) return;
        
        // Find open slot for card
        var slot = localPlayer.GetOpenUpgradeSlot();
        if (slot == -1) return;
        
        shopCard.Remove();
        _shopCards[Array.IndexOf(_shopCards, shopCard)] = null;
        ShopPhase.Instance.MakeDecision(false, card, slot);
    }

    public void Skip() {
        ShopPhase.Instance.MakeDecision(true, null, -1);
    }
}