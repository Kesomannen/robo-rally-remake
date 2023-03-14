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

    public static string PlayerName { get; private set; } = "Player";
    public static event Action<string> OnNameChanged;
    
    static readonly Dictionary<ulong, LobbyPlayerData> _playersInLobby = new();
    public static IReadOnlyDictionary<ulong, LobbyPlayerData> PlayersInLobby => _playersInLobby;
    
    public static LobbySettings LobbySettings { get; private set; } = new();
    public static readonly ObservableField<int> LobbyMap = new();

    public static event Action<ulong, LobbyPlayerData> OnPlayerUpdatedOrAdded;
    public static event Action<ulong> OnPlayerRemoved;
    public static event Action<LobbySettings> OnLobbySettingsUpdated;

    public static string LobbyJoinCode => Matchmaking.CurrentLobby.LobbyCode;
    
    const string PlayerPrefsNameKey = "PlayerName";
    
    void Start() {
        NetworkObject.DestroyWithScene = true;

        using (new LoadingScreen("Signing in...")) {
            Matchmaking.InitializeAsync();
        }
        
        _inputField.onSubmit.AddListener(s => {
            PlayerName = s;
            PlayerPrefs.SetString(PlayerPrefsNameKey, s);
            _enterNameMenu.SetActive(false);
            OnNameChanged?.Invoke(s);
        });
        
        if (PlayerPrefs.HasKey(PlayerPrefsNameKey)) {
            PlayerName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        } else {
            GatherName();   
        }
    }

    public void GatherName() {
        _enterNameMenu.SetActive(true);
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        LobbyMap.Value = Matchmaking.CurrentMapID;
        _playersInLobby.Clear();
        
        var id = NetworkManager.LocalClientId;
        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;

            LobbySettings = new LobbySettings();
            _playersInLobby[id] = new LobbyPlayerData {
                Name = PlayerName,
                IsHost = true,
                IsReady = false,
                RobotId = (byte)RobotData.GetRandom().GetLookupId()
            };
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
        _playersInLobby[id] = new LobbyPlayerData {
            Name = "Retrieving name...",
            IsHost = false,
            IsReady = false,
            RobotId = GetRandomRobot()
        };
        OnPlayerUpdatedOrAdded?.Invoke(id, _playersInLobby[id]);
        
        GetNameClientRpc(id);
        Invoke(nameof(SendLobbyUpdates), 0.5f);
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
        UpdateLobbySettingsClientRpc(LobbySettings);
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
        SetNameServerRpc(id, PlayerName);
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

    [ClientRpc]
    void UpdateLobbySettingsClientRpc(LobbySettings settings) {
        if (IsServer) return;
        LobbySettings = settings;
        OnLobbySettingsUpdated?.Invoke(settings);
    }

    # region Public Methods

    public void RefreshLobbySettings() {
        OnLobbySettingsUpdated?.Invoke(LobbySettings);
        Debug.Log($"Refreshed lobby settings: {LobbySettings}");
        UpdateLobbySettingsClientRpc(LobbySettings);
    }
    
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
    
    public async Task LeaveLobby() {
        try {
            NetworkManager.Shutdown();
            await Matchmaking.LeaveLobbyAsync();   
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to leave lobby: {e.Message}");
        }
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

    public IEnumerator StartGame() {
        Debug.Log("Starting game");
        var lockLobbyAsync = Matchmaking.LockLobbyAsync();
        yield return new WaitUntil(() => lockLobbyAsync.IsCompleted);
        
        var sceneManager = NetworkManager.SceneManager;
        var succeeded = false;
        var failed = false;
        
        sceneManager.OnLoadEventCompleted += LoadComplete;
        sceneManager.LoadScene("Game", LoadSceneMode.Single);
        yield return new WaitUntil(() => succeeded || failed);

        void LoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
            sceneManager.OnLoadEventCompleted -= LoadComplete;
            if (clientsTimedOut.Count > 0) {
                Debug.LogError($"Failed to load scene {sceneName} for clients {string.Join(", ", clientsTimedOut)}");
                failed = true;
            } else {
                succeeded = true;
            }
        }   
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