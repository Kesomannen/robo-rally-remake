using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

#pragma warning disable 4014

public class LobbySystem : NetworkBehaviour {
    public static LobbySystem Instance { get; private set; }

    static Dictionary<ulong, PlayerData> _playersInLobby = new();
    public static IReadOnlyDictionary<ulong, PlayerData> PlayersInLobby => _playersInLobby;

    public static byte LobbyMapId { get; private set; }
    
    public static event Action<ulong, PlayerData> OnPlayerUpdatedOrAdded;
    public static event Action<ulong> OnPlayerRemoved;

    void Awake() {
        Instance = this;
    }

    void Start() {
        NetworkObject.DestroyWithScene = true;

        Matchmaking.InitializeAsync();
    }

    public override void OnDestroy() {
        base.OnDestroy();
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    # region Public Methods

    public async Task<bool> LeaveLobby() {
        Debug.Log("Leaving Lobby");
        try {
            _playersInLobby.Clear();
            NetworkManager.Shutdown();
            await Matchmaking.LeaveLobbyAsync();
            return true;
        } catch (Exception e) {
            Debug.LogError($"Failed to leave lobby: {e.Message}");
            return false;
        }
    }

    public async Task<bool> JoinLobby(string lobbyCode) {
        Debug.Log($"Joining lobby {lobbyCode}");
        try {
            await Matchmaking.JoinLobbyWithCodeAsync(lobbyCode);
            NetworkManager.StartClient();
            return true;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            return false;
        }
    }

    public async Task<bool> CreateLobby(LobbyData lobbyData) {
        Debug.Log("Creating lobby");
        try {
            await Matchmaking.CreateLobbyAndAllocationAsync(lobbyData);
            NetworkManager.StartHost();
            return true;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return false;
        }
    }

    public void UpdatePlayerData(bool? ready = null, byte? robotId = null) {
        Debug.Log($"Updating player data: ready: {ready}, robotId: {robotId}");
        var id = NetworkManager.LocalClientId;
        var playerData = _playersInLobby[id];
        if (ready != null) {
            playerData.IsReady = ready.Value;
        }
        if (robotId != null) {
            playerData.RobotId = robotId.Value;
        }
        UpdatePlayerServerRpc(id, playerData);
    }

    public async void StartGame() {
        Debug.Log("Starting game");
        await Matchmaking.LockLobbyAsync();
        NetworkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    # endregion

    [ServerRpc(RequireOwnership = false)]
    void UpdatePlayerServerRpc(ulong playerId, PlayerData newPlayerData) {
        _playersInLobby[playerId] = newPlayerData;
        UpdatePlayerClientRpc(playerId, newPlayerData);
        OnPlayerUpdatedOrAdded?.Invoke(playerId, newPlayerData);
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;

            var id = NetworkManager.LocalClientId;
            _playersInLobby.Add(id, new PlayerData() { IsHost = true });
            OnPlayerUpdatedOrAdded?.Invoke(id, _playersInLobby[id]);
        }

        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnClientConnected(ulong player) {
        if (!IsServer) return;

        _playersInLobby[player] = new();
        SendLobbyUpdates();
        OnPlayerUpdatedOrAdded?.Invoke(player, _playersInLobby[player]);
    }

    void SendLobbyUpdates() {
        foreach (var player in _playersInLobby) {
            UpdatePlayerClientRpc(player.Key, player.Value);
        }
    }

    [ClientRpc]
    void UpdatePlayerClientRpc(ulong player, PlayerData data) {
        if (IsServer) return;
        _playersInLobby[player] = data;
        OnPlayerUpdatedOrAdded?.Invoke(player, data);
    }

    void OnClientDisconnected(ulong player) {
        if (IsServer) {
            _playersInLobby.Remove(player);
            RemovePlayerClientRpc(player);
            OnPlayerRemoved?.Invoke(player);
        } else {
            LeaveLobby();
        } 
    }

    [ClientRpc]
    void RemovePlayerClientRpc(ulong player) {
        if (IsServer) return;
        _playersInLobby.Remove(player);
        OnPlayerRemoved?.Invoke(player);
    }
}

public struct PlayerData : INetworkSerializable {
    public bool IsReady;
    public bool IsHost;
    public byte RobotId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref IsHost);
        serializer.SerializeValue(ref RobotId);
    }
}