using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem> {
    [SerializeField] PlayerModel _playerModelPrefab;

    static readonly List<Player> _players = new();
    public static IReadOnlyList<Player> Players => _players;
    public static Player LocalPlayer { get; private set; }

    static List<RebootToken> _unoccupiedSpawnPoints;

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
        _unoccupiedSpawnPoints = MapSystem
            .GetByType<RebootToken>()
            .Where(x => x.IsSpawnPoint)
            .ToList();
    }

    public void CreatePlayer(ulong id, LobbyPlayerData data, bool randomizeSpawn) {
        var settings = GameSettings.Instance;

        var robotData = RobotData.GetById(data.RobotId);
        
        RebootToken spawnPoint;
        if (randomizeSpawn) {
            var randomIndex = Random.Range(0, _unoccupiedSpawnPoints.Count);
            spawnPoint = _unoccupiedSpawnPoints[randomIndex];
            _unoccupiedSpawnPoints.RemoveAt(randomIndex);
        } else {
            spawnPoint = _unoccupiedSpawnPoints[0];
            _unoccupiedSpawnPoints.RemoveAt(0);
        }
        
        var playerArgs = new PlayerArgs {
            RobotData = robotData,
            ModelPrefab = _playerModelPrefab,
            SpawnPoint = spawnPoint,
            StartingEnergy = settings.StartingEnergy,
            CardsPerTurn = settings.CardsPerTurn,
            HandSize = settings.MaxCardsInHand,
            RegisterCount = ExecutionPhase.RegisterCount,
            RebootAffector = settings.RebootAffector.ToInstance(),
            UpgradeSlots = settings.UpgradeSlots,
            Name = data.Name
        };

        var newPlayer = new Player(playerArgs);
        _players.Add(newPlayer);
    
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null || networkManager.LocalClientId == id) {
            LocalPlayer = newPlayer;
        }

        Debug.Log($"Created player for client {id}");
    }

    public static IEnumerable<Player> GetOrderedPlayers() {
        var players = _players.ToDictionary(x => x.BonusPriority)
                              .OrderBy(x => Antenna.GetDistance(x.Key.Model.GridPos))
                              .ThenByDescending(x => x.Key.Model.GridPos.y)
                              .ToDictionary(x => x.Key, x => x.Value);

        for (var i = 0; i < players.Count; i++) {
            var player = players.ElementAt(i);
            players[player.Key] += _players.Count - i;
        }

        return players
            .OrderByDescending(x => x.Value)
            .ThenByDescending(x => x.Key.BonusPriority)
            .Select(x => x.Key);
    }
}