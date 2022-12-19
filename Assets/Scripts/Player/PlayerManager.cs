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

    public static bool IsLocal(Player player) => player == LocalPlayer;
    
    protected override void Awake() {
        base.Awake();
        MapSystem.OnMapLoaded += OnMapLoaded;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        MapSystem.OnMapLoaded -= OnMapLoaded;
    }

    static void OnMapLoaded() {
        _spawnPoints = MapSystem.GetByType<RebootToken>().Where(x => x.IsSpawnPoint).ToArray();
    }

    public Player CreatePlayer(ulong id, LobbyPlayerData data) {
        var index = _players.Count;
        var settings = GameSettings.Instance;

        var robotData = RobotData.GetById(data.RobotId);
        var spawnPoint = _spawnPoints[index];

        var playerArgs = new PlayerArgs {
            OwnerId = id,
            RobotData = robotData,
            ModelPrefab = _playerModelPrefab,
            SpawnPoint = spawnPoint,
            StartingEnergy = settings.StartingEnergy,
            CardsPerTurn = settings.CardsPerTurn,
            HandSize = settings.MaxCardsInHand,
            RegisterCount = ExecutionPhase.RegisterCount,
            RebootAffector = settings.RebootAffector.ToInstance(),
            UpgradeSlots = settings.UpgradeSlots,
        };

        var newPlayer = new Player(playerArgs);
        _players.Add(newPlayer);
    
        var singleton = NetworkManager.Singleton;
        if (singleton == null || singleton.LocalClientId == id) {
            LocalPlayer = newPlayer;
        }

        Debug.Log($"Created player for client {id}");
        
        return newPlayer;
    }

    public static IEnumerable<Player> GetOrderedPlayers() {
        var players = _players.ToDictionary(x => x.BonusPriority)
                              .OrderBy(x => Antenna.GetDistance(x.Key.Model.GridPos))
                              .ToDictionary(x => x.Key, x => x.Value);

        for (var i = 0; i < players.Count; i++) {
            var player = players.ElementAt(i);
            players[player.Key] += _players.Count - i;
        }

        return players
            .OrderBy(x => x.Value)
            .Select(x => x.Key);
    }

    public static RebootToken GetSpawnPoint(Player owner) {
        return _spawnPoints[_players.IndexOf(owner)];
    }
}