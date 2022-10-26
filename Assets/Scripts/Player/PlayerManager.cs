using UnityEngine;

public class PlayerManager : Singleton<PlayerManager> {
    [Header("References")]
    [SerializeField] PlayerModel _playerModelPrefab;
    [SerializeField] Transform[] _spawnPoints;

    [Header("Metadata")]
    [SerializeField] int _playerCount;

    [Header("Stats")]
    [SerializeField] int _maxEnergy;
    [SerializeField] int _startingEnergy;
    [SerializeField] int _handSize;
    [SerializeField] int _cardsToDraw;
    [SerializeField] ProgramCardData[] _startingDeck;

    public int MaxEnergy => _maxEnergy;
    public int StartingEnergy => _startingEnergy;
    public int HandSize => _handSize;
    public int CardsToDraw => _cardsToDraw;
    public ProgramCardData[] StartingDeck => _startingDeck;

    public PlayerModel PlayerModelPrefab => _playerModelPrefab;

    public static int PlayerCount => instance._playerCount;
    public static Player[] Players { get; private set; }

    void Start() {
        Players = new Player[_playerCount];
        for (int i = 0; i < _playerCount; i++) {
            var pos = MapSystem.instance.GetGridPos(_spawnPoints[i].position);
            Players[i] = new Player(pos);
        }
    }
}