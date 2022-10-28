using System;
using System.Collections.Generic;
using System.Linq;
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

    public static int PlayerCount => Instance._playerCount;
    static Player[] _players;
    public static IReadOnlyList<Player> Players => _players;

    public static event Action<Player[]> OnPlayersCreated;

    protected override void Awake() {
        base.Awake();
        MapSystem.OnInstanceCreated += _ => CreatePlayers();
    }

    void CreatePlayers() {
        _players = new Player[_playerCount];
        for (int i = 0; i < _playerCount; i++) {
            var pos = MapSystem.Instance.GetGridPos(_spawnPoints[i].position);
            _players[i] = new Player(pos);
        }
        OnPlayersCreated?.Invoke(_players);
    }

    public static Player[] OrderPlayers() {
        var orderedPlayers = new Dictionary<Player, int>();
        foreach (var player in _players) {
            var priority = player.GetBonusPriority();
            orderedPlayers.Add(player, priority);
        }
        orderedPlayers = orderedPlayers
                         .OrderBy(x => Antenna.GetDistance(x.Key.Model.GridPos))
                         .ToDictionary(x => x.Key, x => x.Value);
        for (int i = 0; i < orderedPlayers.Count; i++) {
            var player = orderedPlayers.ElementAt(i).Key;
            orderedPlayers[player] += PlayerCount - i;
        }
        return orderedPlayers
               .OrderBy(x => x.Value)
               .Select(x => x.Key).ToArray();
    }
}