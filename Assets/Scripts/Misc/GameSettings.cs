using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/Settings/Game", order = 0)]
public class GameSettings : SingletonSO<GameSettings> {
    [Header("General")]
    [SerializeField] int _maxPlayers;
    [SerializeField] int _minPlayers;

    [Header("Energy")]
    [SerializeField] int _startingEnergy;
    [SerializeField] int _maxEnergy;

    [Header("Programming")]
    [SerializeField] int _cardsPerTurn;
    [SerializeField] int _maxCardsInHand;
    [SerializeField] int _stressTime;

    [Header("Damaging")]
    [SerializeField] Damage _standardRebootDamage;

    public int MaxPlayers => _maxPlayers;
    public int MinPlayers => _minPlayers;

    public int StartingEnergy => _startingEnergy;
    public int MaxEnergy => _maxEnergy;

    public int CardsPerTurn => _cardsPerTurn;
    public int MaxCardsInHand => _maxCardsInHand;
    public int StressTime => _stressTime;

    public Damage StandardRebootDamage => _standardRebootDamage;
}