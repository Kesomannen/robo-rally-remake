using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem> {
    static List<Player> _players;
    public static IReadOnlyList<Player> Players => _players;
    public static Player LocalPlayer { get; private set; }

    public static bool IsLocal(Player player) => player == LocalPlayer;

    public static event Action<Player> PlayerRemoved;

    protected override void Awake() {
        base.Awake();
        _players = new List<Player>();
    }

    public static void RemovePlayer(Player player) {
        MapSystem.DestroyObject(player.Model);
        
        PlayerRemoved?.Invoke(player);
        _players.Remove(player);
    }
    
    public static void CreatePlayer(ulong id, RobotData robot, string playerName) {
        var settings = GameSystem.Settings;

        var playerArgs = new PlayerArgs {
            RobotData = robot,
            StartingEnergy = settings.StartingEnergy,
            CardsPerTurn = settings.CardsPerTurn,
            RegisterCount = ExecutionPhase.RegisterCount,
            UpgradeSlots = settings.UpgradeSlots,
            Name = playerName,
            ClientId = id
        };

        var newPlayer = new Player(playerArgs);
        _players.Add(newPlayer);
        
        if (NetworkManager.Singleton.LocalClientId == id) {
            LocalPlayer = newPlayer;
        }

        Debug.Log($"Created player {newPlayer}");
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