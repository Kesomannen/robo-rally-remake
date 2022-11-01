using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public interface IPlayerManager {
    int MaxEnergy { get; }
    int StartingEnergy { get; }
    int HandSize { get; }
    ProgramCardData[] StartingDeck { get; }
    PlayerModel PlayerModelPrefab { get; }
}

public class PlayerManager : Singleton<PlayerManager>, IPlayerManager {
    [Header("References")]
    [SerializeField] PlayerModel _playerModelPrefab;
    [SerializeField] Transform[] _spawnPoints;

    [Header("Metadata")]
    [SerializeField] int _playerCount;

    [Header("Stats")]
    [SerializeField] int _maxEnergy;
    [SerializeField] int _startingEnergy;
    [SerializeField] int _handSize;
    [SerializeField] ProgramCardData[] _startingDeck;

    public int MaxEnergy => _maxEnergy;
    public int StartingEnergy => _startingEnergy;
    public int HandSize => _handSize;
    public ProgramCardData[] StartingDeck => _startingDeck;
    public PlayerModel PlayerModelPrefab => _playerModelPrefab;

    public static int PlayerCount => Instance._playerCount;

    static GamePlayer[] _players;
    public static IReadOnlyList<GamePlayer> Players => _players;
    public static GamePlayer LocalPlayer { get; private set; }

    public static event Action<GamePlayer[]> OnPlayersCreated;

    public void CreatePlayers(ulong[] playerIds) {
        _players = new GamePlayer[_playerCount];
        for (int i = 0; i < _playerCount; i++) {
            _players[i] = new GamePlayer(
                _spawnPoints[i].position,
                playerIds[i],
                this
            );
            if (playerIds[i] == NetworkManager.Singleton.LocalClientId) {
                LocalPlayer = _players[i];
            }
        }
        OnPlayersCreated?.Invoke(_players);
    }

    public static GamePlayer[] OrderPlayers() {
        var orderedPlayers = new Dictionary<GamePlayer, int>();
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