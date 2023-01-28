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
        ShopPhase.OnNewPlayer += OnNewPlayer;
    }

    void OnDestroy() {
        ShopPhase.OnRestock -= OnRestock;
        ShopPhase.OnPlayerDecision -= OnPlayerDecision;
        ShopPhase.OnNewPlayer -= OnNewPlayer;
    }
    
    void OnNewPlayer(Player player) {
        
    }

    void OnRestock(int index, UpgradeCardData card) {
        TaskScheduler.PushRoutine(Wrapper());
        
        IEnumerator Wrapper() {
            var shopCard = _shopCards[index];
            if (shopCard != null) {
                shopCard.OnCardClicked -= OnCardClicked;
                yield return shopCard.DisappearAnimation();
            }
            var newCard = Instantiate(_shopCardPrefab, _upgradeParent);
            newCard.transform.SetSiblingIndex(index);
            _shopCards[index] = newCard;
        
            newCard.SetContent(card);
            newCard.OnCardClicked += OnCardClicked;

            yield return newCard.RestockAnimation();
        }
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
        TaskScheduler.PushRoutine(Wrapper());
        
        IEnumerator Wrapper() {
            if (skipped) {
                
            } else {
                for (var i = 0; i < _shopCards.Length; i++){
                    var shopCard = _shopCards[i];
                    if (shopCard == null || shopCard.Content != card) continue;

                    yield return shopCard.BuyAnimation();
                    _shopCards[i] = null;
                    yield break;
                }   
            }
        }
    }

    // Used in button UnityEvent
    public void Skip() {
        ShopPhase.Instance.MakeDecision(true, null, -1);
    }
}