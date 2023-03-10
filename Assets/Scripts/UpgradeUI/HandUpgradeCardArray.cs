using System.Linq;
using UnityEngine;

public class HandUpgradeCardArray : MonoBehaviour {
    [SerializeField] HandUpgradeCard _cardPrefab;
    [SerializeField] Transform _cardParent;
    [SerializeField] Vector2 _cardSize;
    [SerializeField] Transform _highlightParent;
    [SerializeField] int _cardsPerRow;

    HandUpgradeCard[] _cards;

    static Player Owner => PlayerSystem.LocalPlayer;

    void Start() {
        _cards = new HandUpgradeCard[Owner.Upgrades.Count];
        for (var i = 0; i < Owner.Upgrades.Count; i++) {
            var upgrade = Owner.Upgrades[i];
            if (upgrade == null) continue;
            CreateCard(upgrade, i);
        }
        Owner.OnUpgradeAdded += CreateCard;
        Owner.OnUpgradeRemoved += RemoveCard;
        
        PhaseSystem.Current.OnValueChanged += OnPhaseChanged;
        ExecutionPhase.OnPlayerRegistersComplete += UpdateAvailability;
        ExecutionPhase.OnPlayerRegister += OnPlayerRegister;
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerLockedIn;
        
        Debug.Log("HandUpgradeCardArray.Start", this);
    }

    void OnDestroy() {
        Owner.OnUpgradeAdded -= CreateCard;
        Owner.OnUpgradeRemoved -= RemoveCard;
        
        PhaseSystem.Current.OnValueChanged -= OnPhaseChanged;
        ExecutionPhase.OnPlayerRegistersComplete -= UpdateAvailability;
        ExecutionPhase.OnPlayerRegister += OnPlayerRegister;
        ProgrammingPhase.OnPlayerLockedIn -= OnPlayerLockedIn;
    }
    
    void OnPhaseChanged(Phase prev, Phase next) => UpdateAvailability();
    void OnPlayerLockedIn(Player _) => UpdateAvailability();
    void OnPlayerRegister(ProgramExecution _) => UpdateAvailability();

    void CreateCard(UpgradeCardData data, int index) {
        var newCard = Instantiate(_cardPrefab, _cardParent);
        if (!newCard.isActiveAndEnabled) newCard.Awake();
        newCard.GetComponent<Container<UpgradeCardData>>().SetContent(data);
        newCard.HighlightParent = _highlightParent;
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
        var rows = Mathf.CeilToInt(cards.Length / (float)_cardsPerRow);
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

    void UpdateAvailability() {
        foreach (var card in _cards) {
            if (card == null) continue;
            card.UpdateAvailability();
        }
    }
}