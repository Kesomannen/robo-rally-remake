using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Random = UnityEngine.Random;

#pragma warning disable 4014

public class LobbySystem : NetworkSingleton<LobbySystem> {
    static Dictionary<ulong, LobbyPlayerData> _playersInLobby;
    public static IReadOnlyDictionary<ulong, LobbyPlayerData> PlayersInLobby => _playersInLobby;

    public static int LobbyMapId { get; private set; }
    
    public static event Action<ulong, LobbyPlayerData> OnPlayerUpdatedOrAdded;
    public static event Action<ulong> OnPlayerRemoved;
    public static event Action<int> OnLobbyMapChanged;

    public static string LobbyJoinCode => Matchmaking.CurrentLobby.LobbyCode;
    
    void Start() {
        NetworkObject.DestroyWithScene = true;

        Matchmaking.InitializeAsync();
    }

    public override void OnDestroy() {
        base.OnDestroy();
        if (NetworkManager == null) return;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        
        if (!IsServer) return;
        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    # region Public Methods

    public async Task LeaveLobby() {
        Debug.Log("Leaving Lobby");
        try {
            NetworkManager.Shutdown();
            await Matchmaking.LeaveLobbyAsync();
        } catch (Exception e) {
            Debug.LogError($"Failed to leave lobby: {e.Message}");
        }
    }

    public async Task JoinLobby(string lobbyCode) {
        Debug.Log($"Joining lobby {lobbyCode}");
        try {
            await Matchmaking.JoinLobbyWithCodeAsync(lobbyCode);
            NetworkManager.StartClient();
        }catch (Exception e) {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
    }

    public async Task CreateLobby(LobbyData lobbyData) {
        Debug.Log("Creating lobby");
        try {
            await Matchmaking.CreateLobbyAndAllocationAsync(lobbyData);
            NetworkManager.StartHost();
        }
        catch (LobbyServiceException e) {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }

    public void UpdatePlayerData(bool? ready = null, byte? robotId = null) {
        Debug.Log($"Updating player data: ready: {ready}, robotId: {robotId}");
        var id = NetworkManager.LocalClientId;
        var playerData = _playersInLobby[id];
        if (ready.HasValue) {
            playerData.IsReady = ready.Value;
        }
        if (robotId.HasValue) {
            playerData.RobotId = robotId.Value;
        }
        UpdatePlayerServerRpc(id, playerData);
    }

    public async void StartGame() {
        Debug.Log("Starting game");
        await Matchmaking.LockLobbyAsync();
        NetworkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
    
    public void SetLobbyMap(int id) {
        if (!IsServer) {
            Debug.LogError("Only the server can change the lobby map");
            return;
        }
        if (id == LobbyMapId) return;

        LobbyMapId = id;
        OnLobbyMapChanged?.Invoke(id);

        UpdateLobbyMapClientRpc((byte) id);
    }

    # endregion

    [ClientRpc]
    void UpdateLobbyMapClientRpc(byte id) {
        if (IsServer) return;
        LobbyMapId = id;
        OnLobbyMapChanged?.Invoke(id);
    }
    
    [ServerRpc(RequireOwnership = false)]
    void UpdatePlayerServerRpc(ulong playerId, LobbyPlayerData newPlayerData) {
        _playersInLobby[playerId] = newPlayerData;
        UpdatePlayerClientRpc(playerId, newPlayerData);
        OnPlayerUpdatedOrAdded?.Invoke(playerId, newPlayerData);
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        LobbyMapId = Matchmaking.CurrentMapID;
        _playersInLobby = new Dictionary<ulong, LobbyPlayerData>();
        
        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;

            var id = NetworkManager.LocalClientId;
            _playersInLobby.Add(id, new LobbyPlayerData { IsHost = true });
            OnPlayerUpdatedOrAdded?.Invoke(id, _playersInLobby[id]);

            StartCoroutine(UpdateLobbyRoutine());
        }

        // Client uses this in case the host disconnects
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnClientConnected(ulong player) {
        if (!IsServer) return;
        
        _playersInLobby[player] = new LobbyPlayerData {
            IsReady = false,
            RobotId = GetRobotId()
        };
        
        foreach (var (id, data) in _playersInLobby) {
            UpdatePlayerClientRpc(id, data);
        }
        OnPlayerUpdatedOrAdded?.Invoke(player, _playersInLobby[player]);

        byte GetRobotId() {
            var occupiedIds = new HashSet<byte>();
            foreach (var (_, data) in _playersInLobby) {
                occupiedIds.Add(data.RobotId);
            }

            byte id;
            var idCount = RobotData.GetAll().Count();
            
            do { id = (byte)Random.Range(0, idCount); } 
            while (occupiedIds.Contains(id));
            
            return id;
        }
    }

    [ClientRpc]
    void UpdatePlayerClientRpc(ulong player, LobbyPlayerData data) {
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

    // Rate limits at 2 seconds
    const float LobbyUpdateInterval = 5f;
    
    IEnumerator UpdateLobbyRoutine() {
        if (!IsServer) yield break;
        while (NetworkManager != null) {
            yield return CoroutineUtils.Wait(LobbyUpdateInterval);
            if (Matchmaking.CurrentMapID == LobbyMapId) continue;
            var task = Matchmaking.UpdateLobbyAsync(new UpdateLobbyDataOptions {
                MapID = (byte?)LobbyMapId
            });
            yield return new WaitUntil(() => task.IsCompleted);
        }
    }
}

public struct LobbyPlayerData : INetworkSerializable {
    public bool IsReady;
    public bool IsHost;
    public byte RobotId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref IsHost);
        serializer.SerializeValue(ref RobotId);
    }
}