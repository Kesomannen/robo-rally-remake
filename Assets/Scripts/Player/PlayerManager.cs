using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PlayerManager : Singleton<PlayerManager> {
    [SerializeField] PlayerModel _playerModelPrefab;

    static readonly List<Player> _players = new();
    public static IReadOnlyList<Player> Players => _players;
    public static Player LocalPlayer { get; private set; }

    static RebootToken[] _spawnPoints;

    protected override void Awake() {
        base.Awake();
        MapSystem.OnMapLoaded += OnMapLoaded;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        MapSystem.OnMapLoaded -= OnMapLoaded;
    }

    void OnMapLoaded() {
        List<RebootToken> checkpoints = new();
        MapSystem.GetByType(checkpoints);
        _spawnPoints = checkpoints.Where(x => x.IsSpawnPoint).ToArray();
    }

    public void CreatePlayer(ulong id, LobbyPlayerData data) {
        var index = _players.Count;
        var settings = GameSettings.instance;

        var robotData = RobotData.GetById(data.RobotId);
        var spawnPoint = _spawnPoints[index];

        var playerArgs = new PlayerArgs() {
            OwnerId = id,
            RobotData = robotData,
            ModelPrefab = _playerModelPrefab,
            SpawnPoint = spawnPoint,
            StartingEnergy = settings.StartingEnergy,
            HandSize = settings.MaxCardsInHand,
            RegisterCount = ExecutionPhase.RegisterCount,
            RebootDamage = settings.StandardRebootDamage,
        };

        var newPlayer = new Player(playerArgs);
        _players.Add(newPlayer);

        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClientId == id) {
            LocalPlayer = newPlayer;
        }

        Debug.Log($"Created player for client {id}");
    }

    public static IEnumerable<Player> GetOrderedPlayers() {
        var players = _players.ToDictionary(x => x.GetBonusPriority())
                              .OrderBy(x => Antenna.GetDistance(x.Key.Model.GridPos))
                              .ToDictionary(x => x.Key, x => x.Value);

        for (var i = 0; i < players.Count; i++) {
            var player = players.ElementAt(i).Key;
            players[player] += _players.Count - i;
        }

        return players
            .OrderBy(x => x.Value)
            .Select(x => x.Key);
    }

    public static RebootToken GetSpawnPoint(Player owner) {
        return _spawnPoints[_players.IndexOf(owner)];
    }
}