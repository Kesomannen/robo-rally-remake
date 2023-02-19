using System.Linq;
using UnityEngine;

public class PlayerUIUpgrades : MonoBehaviour {
    [SerializeField] Container<UpgradeCardData> _cardPrefab;
    [SerializeField] Transform _cardParent;
    [SerializeField] Vector2 _cardSize;
    [SerializeField] Transform _highlightParent;

    Container<UpgradeCardData>[] _cards;

    static Player Owner => PlayerSystem.LocalPlayer;

    void Awake() {
        _cards = new Container<UpgradeCardData>[Owner.Upgrades.Count];
        for (var i = 0; i < Owner.Upgrades.Count; i++) {
            var upgrade = Owner.Upgrades[i];
            if (upgrade == null) continue;
            CreateCard(upgrade, i);
        }
        Owner.OnUpgradeAdded += CreateCard;
        Owner.OnUpgradeRemoved += RemoveCard;
    }

    void OnDestroy() {
        Owner.OnUpgradeAdded -= CreateCard;
        Owner.OnUpgradeRemoved -= RemoveCard;
    }

    void CreateCard(UpgradeCardData data, int index) {
        var newCard = Instantiate(_cardPrefab, _cardParent);
        newCard.SetContent(data);
        newCard.GetComponent<HandUpgradeCard>().HighlightParent = _highlightParent;
        _cards[index] = newCard;
        
        newCard.transform.SetSiblingIndex(index);
        UpdatePositions();
    }
    
    void RemoveCard(UpgradeCardData data, int index) {
        Destroy(_cards[index].gameObject);
        _cards[index] = null;
        UpdatePositions();
    }
    
    void UpdatePositions() {
        var cards = _cards.Where(c => c != null).ToArray();
        var rows = Mathf.CeilToInt(cards.Length / 3f);
        var columns = Mathf.CeilToInt(cards.Length / (float)rows);
        
        var i = 0;
        for (var row = 0; row < rows; row++) {
            for (var column = 0; column < columns; column++) {
                if (i >= cards.Length) return;
                
                var panel = cards[i];
                var position = new Vector2(column * _cardSize.x, -row * _cardSize.y);
                panel.transform.localPosition = position;
                panel.transform.SetSiblingIndex(i);
                i++;
            }
        }
    }
}