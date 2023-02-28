using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.Services.Lobbies;
using Random = UnityEngine.Random;

#pragma warning disable 4014

public class LobbySystem : NetworkSingleton<LobbySystem> {
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] GameObject _enterNameMenu;
    
    static string _playerName;
    
    static readonly Dictionary<ulong, LobbyPlayerData> _playersInLobby = new();
    public static IReadOnlyDictionary<ulong, LobbyPlayerData> PlayersInLobby => _playersInLobby;

    public static readonly ObservableField<int> LobbyMap = new();

    public static event Action<ulong, LobbyPlayerData> OnPlayerUpdatedOrAdded;
    public static event Action<ulong> OnPlayerRemoved;

    public static string LobbyJoinCode => Matchmaking.CurrentLobby.LobbyCode;
    
    const string PlayerPrefsNameKey = "PlayerName";
    
    void Start() {
        NetworkObject.DestroyWithScene = true;

        using (new LoadingScreen("Signing in...")) {
            Matchmaking.InitializeAsync();
        }
        GetName();
    }

    void GetName() {
        if (PlayerPrefs.HasKey(PlayerPrefsNameKey)) {
            _playerName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        } else {
            _enterNameMenu.SetActive(true);
            _inputField.onSubmit.AddListener(s => {
                _playerName = s;
                PlayerPrefs.SetString(PlayerPrefsNameKey, s);
                _enterNameMenu.SetActive(false);
            });
        }
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        LobbyMap.Value = Matchmaking.CurrentMapID;
        _playersInLobby.Clear();
        
        var id = NetworkManager.LocalClientId;
        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            
            _playersInLobby.Add(id, new LobbyPlayerData {
                Name = _playerName,
                IsHost = true,
                IsReady = false,
                RobotId = (byte) RobotData.GetRandom().GetLookupId()
            });
            OnPlayerUpdatedOrAdded?.Invoke(id, _playersInLobby[id]);

            //StartCoroutine(UpdateLobbyRoutine());
        }

        // Client uses this in case the host disconnects
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }
    
    public override void OnDestroy() {
        base.OnDestroy();
        
        // We only care about this in lobby
        if (NetworkManager == null) return;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;

        if (!IsServer) return;
        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }
    
    void OnClientConnected(ulong id) {
        if (!IsServer) return;
        _playersInLobby.Add(id, new LobbyPlayerData {
            Name = "Retrieving name...",
            IsHost = false,
            IsReady = false,
            RobotId = GetRandomRobot()
        });
        OnPlayerUpdatedOrAdded?.Invoke(id, _playersInLobby[id]);
        
        GetNameClientRpc(id);
        SendLobbyUpdates();
    }
    
    void OnClientDisconnected(ulong player) {
        if (IsServer) {
            _playersInLobby.Remove(player);
            RemovePlayerClientRpc(player);
            OnPlayerRemoved?.Invoke(player);
        } else {
            // Host disconnected
            LeaveLobby();
        } 
    }

    void SendLobbyUpdates() {
        foreach (var (id, data) in _playersInLobby) {
            UpdatePlayerClientRpc(id, data);
        }
        UpdateLobbyMapClientRpc((byte) LobbyMap.Value);
    }

    [ClientRpc]
    void RemovePlayerClientRpc(ulong player) {
        if (IsServer) return;
        _playersInLobby.Remove(player);
        OnPlayerRemoved?.Invoke(player);
    }
    
    [ClientRpc]
    void GetNameClientRpc(ulong id) {
        if (IsServer || id != NetworkManager.LocalClientId) return;
        SetNameServerRpc(id, _playerName);
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SetNameServerRpc(ulong id, string playerName) {
        var data = _playersInLobby[id];
        data.Name = playerName;
        _playersInLobby[id] = data;
        OnPlayerUpdatedOrAdded?.Invoke(id, data);
        
        UpdatePlayerClientRpc(id, data);
        Debug.Log($"Set name of player {id} to {playerName}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    void UpdatePlayerServerRpc(ulong playerId, LobbyPlayerData newPlayerData) {
        _playersInLobby[playerId] = newPlayerData;
        OnPlayerUpdatedOrAdded?.Invoke(playerId, newPlayerData);
        
        UpdatePlayerClientRpc(playerId, newPlayerData);
    }
    
    [ClientRpc]
    void UpdatePlayerClientRpc(ulong id, LobbyPlayerData data) {
        if (IsServer) return;
        
        _playersInLobby[id] = data;
        OnPlayerUpdatedOrAdded?.Invoke(id, data);
        Debug.Log($"Updated player {id} with data {data}");
    }

    # region Public Methods

    public void UpdatePlayer([CanBeNull] RobotData robot = null, bool? ready = null) {
        var id = NetworkManager.LocalClientId;
        var data = _playersInLobby[id];
        
        if (robot != null) {
            data.RobotId = (byte) robot.GetLookupId();
        }
        if (ready != null) {
            data.IsReady = ready.Value;
        }
        UpdatePlayerServerRpc(id, data);
    }
    
    public async Task<bool> LeaveLobby() {
        try {
            NetworkManager.Shutdown();
            await Matchmaking.LeaveLobbyAsync();   
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to leave lobby: {e.Message}");
            return false;
        }
        return true;
    }

    public async Task<bool> JoinLobby(string lobbyCode) {
        try {
            await Matchmaking.JoinLobbyWithCodeAsync(lobbyCode);   
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            return false;
        }
        NetworkManager.StartClient();
        return true;
    }

    public async Task<bool> CreateLobby(LobbyData lobbyData) {
        try {
            await Matchmaking.CreateLobbyAndAllocationAsync(lobbyData);
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return false;
        }
        
        NetworkManager.StartHost();
        return true;
    }

    public async Task<bool> StartGame() {
        try {
            await Matchmaking.LockLobbyAsync();   
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to lock lobby: {e.Message}");
            return false;
        }
        NetworkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        return true;
    }
    
    public void SetLobbyMap(int id) {
        if (!IsServer || id == LobbyMap.Value) return;

        LobbyMap.Value = id;
        UpdateLobbyMapClientRpc((byte) id);
    }

    [ClientRpc]
    void UpdateLobbyMapClientRpc(byte id) {
        if (IsServer) return;
        LobbyMap.Value = id;
    }
    
    # endregion

    // Rate limits at 2 seconds
    const float LobbyUpdateInterval = 5f;
    
    IEnumerator UpdateLobbyRoutine() {
        if (!IsServer) yield break;
        
        while (NetworkManager != null) {
            if (Matchmaking.CurrentMapID == LobbyMap.Value) continue;
            var task = Matchmaking.UpdateLobbyAsync(new UpdateLobbyDataOptions {
                MapID = (byte?)LobbyMap.Value
            });
            yield return new WaitUntil(() => task.IsCompleted);
            yield return CoroutineUtils.Wait(LobbyUpdateInterval);
        }
    }
    
    static byte GetRandomRobot() {
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

public struct LobbyPlayerData : INetworkSerializable {
    public bool IsReady;
    public bool IsHost;
    public byte RobotId;
    public string Name;

    public override string ToString() {
        return $"{{IsReady: {IsReady}, IsHost: {IsHost}, RobotId: {RobotId}, Name: {Name}}}";
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref IsHost);
        serializer.SerializeValue(ref RobotId);
        serializer.SerializeValue(ref Name);
    }
}