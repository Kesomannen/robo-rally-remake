using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager> {
    [Header("References")]
    [SerializeField] PlayerModel _playerModelPrefab;
    [SerializeField] Checkpoint[] _spawnPoints;

    [Header("Stats")]
    [SerializeField] int _maxEnergy;
    [SerializeField] int _startingEnergy;
    [SerializeField] int _handSize;

    public int MaxEnergy => _maxEnergy;
    public int StartingEnergy => _startingEnergy;
    public int HandSize => _handSize;
    public PlayerModel PlayerModelPrefab => _playerModelPrefab;

    static readonly List<Player> _players = new();
    public static IReadOnlyList<Player> Players => _players;
    public static Player LocalPlayer { get; private set; }

    protected override void Awake() {
        base.Awake();
    }

    public void CreatePlayer(ulong id, PlayerData data) {
        var index = _players.Count;

        var playerArgs = new PlayerArgs() {
            OwnerId = id,
            RobotData = RobotData.GetById(data.RobotId),
            ModelPrefab = _playerModelPrefab,
            SpawnPoint = _spawnPoints[index],
            MaxEnergy = _maxEnergy,
            StartingEnergy = _startingEnergy,
            HandSize = _handSize
        };

        _players.Add(new(playerArgs));

        if (id == NetworkManager.Singleton.LocalClientId) {
            LocalPlayer = _players[index];
        }

        Debug.Log($"Created player {index} for player {id}");
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
            orderedPlayers[player] += _players.Count - i;
        }

        return orderedPlayers
               .OrderBy(x => x.Value)
               .Select(x => x.Key).ToArray();
    }
}