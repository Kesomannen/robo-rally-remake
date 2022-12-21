using UnityEngine;

[CreateAssetMenu(fileName = "NewGameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableSingleton<GameSettings> {
    [Header("General")]
    [SerializeField] int _maxPlayers;
    [SerializeField] int _minPlayers;

    [Header("Energy")]
    [SerializeField] int _startingEnergy;
    
    [Header("Programming")]
    [SerializeField] int _cardsPerTurn;
    [SerializeField] int _maxCardsInHand;
    [SerializeField] int _stressTime;

    [Header("Damaging")]
    [SerializeField] ScriptableCardAffector _rebootAffector;

    [Header("Upgrades")]
    [SerializeField] int _shopSlots;
    [SerializeField] int _upgradeSlots;

    public int MaxPlayers => _maxPlayers;
    public int MinPlayers => _minPlayers;

    public int StartingEnergy => _startingEnergy;
    
    public int CardsPerTurn => _cardsPerTurn;
    public int MaxCardsInHand => _maxCardsInHand;
    public int StressTime => _stressTime;

    public ScriptableCardAffector RebootAffector => _rebootAffector;
    
    public int ShopSlots => _shopSlots;
    public int UpgradeSlots => _upgradeSlots;
}