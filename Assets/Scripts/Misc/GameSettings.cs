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
    [SerializeField] Container<ProgramCardData> _programCardContainerPrefab;

    [Header("Damaging")]
    [SerializeField] ScriptableCardAffector _rebootAffector;

    [Header("Upgrades")]
    [SerializeField] int _upgradeSlots;
    [SerializeField] Container<UpgradeCardData> _upgradeContainerPrefab;

    public int MaxPlayers => _maxPlayers;
    public int MinPlayers => _minPlayers;

    public int StartingEnergy => _startingEnergy;
    
    public int CardsPerTurn => _cardsPerTurn;
    public int MaxCardsInHand => _maxCardsInHand;
    public int StressTime => _stressTime;
    public Container<ProgramCardData> ProgramCardContainerPrefab => _programCardContainerPrefab;

    public ScriptableCardAffector RebootAffector => _rebootAffector;
    
    public int UpgradeSlots => _upgradeSlots;
    public Container<UpgradeCardData> UpgradeContainerPrefab => _upgradeContainerPrefab;
}