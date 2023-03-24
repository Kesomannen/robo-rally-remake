using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

#pragma warning disable 4014

public class NetworkSystem : NetworkSingleton<NetworkSystem> {
    [SerializeField] GameObject _waitingOverlay;
    [SerializeField] TMP_Text _waitingText;
    
    public static Context LoadContext { get; set; } = Context.Singleplayer;
    public static LobbyPlayerData SingleplayerPlayerData { get; set; } = new() {
        IsHost = true,
        RobotId = 2,
        Name = "Player"
    };

    public static GameSystem.GameOptions? GameOptions { get; set; }

    public enum Context {
        Singleplayer,
        Multiplayer
    }
    
    void Start() {
        NetworkObject.DestroyWithScene = true;
        
        if (LoadContext != Context.Singleplayer) return;
        NetworkManager.StartHost();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        Debug.Log("NetworkSystem spawned, loading map...");
        MapSystem.Instance.LoadMap(MapData.GetById(LobbySystem.LobbyMap.Value));
        
        if (LoadContext == Context.Singleplayer) {
            PlayerSystem.Instance.CreatePlayer(NetworkManager.LocalClientId, SingleplayerPlayerData);
        } else {
            foreach (var (id, data) in LobbySystem.PlayersInLobby.OrderBy(value => value.Key)) {
                PlayerSystem.Instance.CreatePlayer(id, data);
            }
        }
        
        if (!PlayerSystem.EnergyEnabled) {
            var spaces = MapSystem.GetByType<EnergySpace>().ToArray();
            // ReSharper disable once ForCanBeConvertedToForeach
            // Collection was modified; enumeration operation may not execute.
            for (var i = 0; i < spaces.Length; i++) {
                var energySpace = spaces[i];
                MapSystem.DestroyObject(energySpace);
            }
        }

        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        
        _waitingOverlay.SetActive(true);
        _waitingText.text = "Waiting for players to load...";
        
        if (IsServer) {
            if (LoadContext == Context.Multiplayer && NetworkManager.ConnectedClientsList.Count > 1) {
                StartCoroutine(WaitForPlayers());
            } else {
                StartGame();
            }
        }

        IEnumerator WaitForPlayers() {
            NetworkManager.SceneManager.OnLoadEventCompleted += LoadEventCompleted;
            var loaded = false;
            yield return new WaitUntil(() => loaded);
            StartGameClientRpc();
            StartGame();

            void LoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
                NetworkManager.SceneManager.OnLoadEventCompleted -= LoadEventCompleted;
                if (clientsTimedOut.Count > 0) {
                    Debug.LogError($"Clients timed out: {string.Join(", ", clientsTimedOut)}");
                }

                if (clientsCompleted.Count < NetworkManager.ConnectedClientsList.Count) {
                    Debug.LogError($"Clients did not load: {string.Join(", ", NetworkManager.ConnectedClientsList.Select(c => c.ClientId).Except(clientsCompleted))}");
                }
                loaded = true;
            }
        }
    }

    [ClientRpc]
    void StartGameClientRpc() {
        if (IsServer) return;
        StartGame();
    }
    
    void OnClientDisconnect(ulong id) {
        if (IsServer) {
            var player = PlayerSystem.Players.First(p => p.ClientId == id);
            Log.Instance.RawMessage($"{Log.PlayerString(player)} left the game");
            PlayerSystem.RemovePlayer(player);
            
            PlayerDisconnectedClientRpc(id);
        } else {
            // Host disconnected
            ReturnToLobby();
        }
    }

    [ClientRpc]
    void PlayerDisconnectedClientRpc(ulong id) {
        if (IsServer) return;
        var player = PlayerSystem.Players.First(p => p.ClientId == id);
        Log.Instance.RawMessage($"{Log.PlayerString(player)} left the game");
        PlayerSystem.RemovePlayer(player);
    }

    void StartGame() {
        if (GameOptions.HasValue) {
            GameSystem.Instance.StartGame(GameOptions.Value);
        } else {
            GameSystem.Instance.StartGame(new GameSystem.GameOptions() {
                DoSetupPhase = true,
                ShopEnabled = PlayerSystem.EnergyEnabled
            });
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();

        if (NetworkManager.Singleton == null) return;

        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        
        Matchmaking.LeaveLobbyAsync();
        NetworkManager.Shutdown();
    }

    const int LobbySceneIndex = 0;
    
    public static void ReturnToLobby() {
        SceneManager.LoadScene(LobbySceneIndex);
    }

    public void BroadcastUpgrade(int index) {
        UseUpgradeServerRpc(
            (byte) PlayerSystem.Players.IndexOf(PlayerSystem.LocalPlayer),
            (byte) index
            );
    }

    [ServerRpc(RequireOwnership = false)]
    void UseUpgradeServerRpc(byte playerIndex, byte upgradeIndex) {
        UseUpgradeClientRpc(playerIndex, upgradeIndex);
        if (PlayerSystem.Players.IndexOf(PlayerSystem.LocalPlayer) == playerIndex) return;
        PlayerSystem.Players[playerIndex].UseUpgrade(upgradeIndex);
    }
    
    [ClientRpc]
    void UseUpgradeClientRpc(byte playerIndex, byte upgradeIndex) {
        if (IsServer || PlayerSystem.Players.IndexOf(PlayerSystem.LocalPlayer) == playerIndex) return;
        PlayerSystem.Players[playerIndex].UseUpgrade(upgradeIndex);
    }
    
    readonly List<ulong> _playersReady = new();

    public IEnumerator SyncPlayers() {
        _waitingOverlay.SetActive(true);
        PlayerReadyServerRpc(NetworkManager.LocalClientId);
        while (_playersReady.Count < PlayerSystem.Players.Count) {
            _waitingText.text = $"Waiting for players... ({_playersReady.Count}/{PlayerSystem.Players.Count})";
            yield return null;
        }
        _waitingOverlay.SetActive(false);
        _playersReady.Clear();
    }
    
    [ServerRpc(RequireOwnership = false)]
    void PlayerReadyServerRpc(ulong id) {
        _playersReady.Add(id);
        PlayerReadyClientRpc(id);
    }
    
    [ClientRpc]
    void PlayerReadyClientRpc(ulong id) {
        if (IsServer) return;
        _playersReady.Add(id);
    }

    static readonly Queue<ProgramCardData[]> _queryResults = new();
    static bool _querying;

    [ClientRpc]
    void QueryClientRpc(byte playerIndex, byte pile, byte startIndex, byte endIndex) {
        var localPlayer = PlayerSystem.LocalPlayer;
        if (PlayerSystem.Players[playerIndex] != localPlayer) return;
        Debug.Log($"Querying {PlayerSystem.Players[playerIndex]}'s {(Pile)pile} pile from {startIndex} to {endIndex}");
        
        if (!GetQuery((Pile) pile, startIndex, endIndex, localPlayer, out var result)) {
            return;
        }

        SendQueryResultsServerRpc(result);
    }
    
    static bool GetQuery(Pile pile, int startIndex, int endIndex, Player player, out byte[] result) {
        var depth = endIndex - startIndex;
        var collection = player.GetCollection(pile);
        
        if (collection.Cards.Count <= startIndex + depth) {
            if (pile == Pile.DrawPile) {
                player.ShuffleDeck();
            } else {
                Debug.LogError($"Cannot query {player}'s {pile} pile from {startIndex} to {endIndex}; not enough cards");
                result = null;
                return false;
            }
        }

        result = collection.Cards
            .Skip(startIndex)
            .Take(depth)
            .Select(c => (byte)c.GetLookupId())
            .ToArray();
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    void SendQueryResultsServerRpc(byte[] cardIds) {
        var cards = cardIds.Select(c => ProgramCardData.GetById(c)).ToArray();
        _queryResults.Enqueue(cards);
        SendQueryResultsClientRpc(cardIds);
    }
    
    [ClientRpc]
    void SendQueryResultsClientRpc(byte[] cardIds) {
        if (IsServer) return;
        var cards = cardIds.Select(c => ProgramCardData.GetById(c)).ToArray();
        _queryResults.Enqueue(cards);
        Debug.Log($"Received query results: {string.Join(", ", cards.Select(c => c.Name))}");
    }

    public IEnumerator QueryPlayerCards(Player player, Pile pile, int startIndex, int endIndex, List<ProgramCardData> result) {
        if (_querying) throw new InvalidOperationException("Cannot query while another query is in progress.");
        _querying = true;
        
        if (IsServer) {
            var playerIndex = PlayerSystem.Players.IndexOf(player);
            QueryClientRpc((byte) playerIndex, (byte) pile, (byte) startIndex, (byte) endIndex);
        }
        yield return new WaitUntil(() => _queryResults.Count > 0);
        
        result.AddRange(_queryResults.Dequeue());
        _querying = false;
    }
}