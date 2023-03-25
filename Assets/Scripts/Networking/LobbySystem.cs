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
    [SerializeField] int _minNameLength = 3;
    [Space]
    [SerializeField] MapData[] _availableMaps;
    [SerializeField] RobotData[] _availableRobots;
    
    public IReadOnlyList<MapData> AvailableMaps => _availableMaps;
    public IReadOnlyList<RobotData> AvailableRobots => _availableRobots;

    static string _playerName;
    public static string PlayerName {
        get {
            if (!string.IsNullOrEmpty(_playerName)) return _playerName;

            if (PlayerPrefs.HasKey(PlayerPrefsNameKey)) {
                _playerName = PlayerPrefs.GetString(PlayerPrefsNameKey);
            } else {
                if (!InstanceExists) return "Player";

                Instance.GatherName();
                return "Retrieving name...";
            }
            return _playerName;
        }
    }
    public static event Action<string> NameChanged;
    
    static readonly Dictionary<ulong, LobbyPlayerData> _playersInLobby = new();
    public static IReadOnlyDictionary<ulong, LobbyPlayerData> PlayersInLobby => _playersInLobby;
    
    public static GameSettings GameSettings { get; private set; } = new();
    public static readonly ObservableField<int> LobbyMap = new();

    public static event Action<ulong, LobbyPlayerData> PlayerUpdatedOrAdded;
    public static event Action<ulong> PlayerRemoved;
    public static event Action<GameProperty> LobbySettingsPropertyUpdated;

    public static string LobbyJoinCode => Matchmaking.CurrentLobby.LobbyCode;
    
    const string PlayerPrefsNameKey = "PlayerName";
    public const int MinPlayers = 1;
    public const int MaxPlayers = 4;
    
    void Start() {
        NetworkObject.DestroyWithScene = true;

        using (new LoadingScreen("Signing in...")) {
            Matchmaking.InitializeAsync();
        }
        Debug.Log($"Signed in as {PlayerName}");

        _inputField.onSubmit.AddListener(str => {
            if (str.Replace(" ", "").Length < _minNameLength) return;
            
            _playerName = str;
            PlayerPrefs.SetString(PlayerPrefsNameKey, str);
            PlayerPrefs.Save();
            
            _enterNameMenu.SetActive(false);
            NameChanged?.Invoke(str);
        });
    }

    public void GatherName() {
        _enterNameMenu.SetActive(true);
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        LobbyMap.Value = Matchmaking.CurrentMapID;
        _playersInLobby.Clear();
        
        if (IsServer) {
            var id = NetworkManager.LocalClientId;
            NetworkManager.OnClientConnectedCallback += OnClientConnected;

            GameSettings = new GameSettings();
            _playersInLobby[id] = new LobbyPlayerData {
                Name = PlayerName,
                IsHost = true,
                IsReady = false,
                RobotId = (byte) AvailableRobots.GetRandom().GetLookupId()
            };
            PlayerUpdatedOrAdded?.Invoke(id, _playersInLobby[id]);

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
        PlayerUpdatedOrAdded?.Invoke(id, _playersInLobby[id]);
        
        GetNameClientRpc(id);
        Invoke(nameof(SendLobbyUpdates), 0.1f);
    }
    
    void OnClientDisconnected(ulong player) {
        if (IsServer) {
            _playersInLobby.Remove(player);
            RemovePlayerClientRpc(player);
            PlayerRemoved?.Invoke(player);
        } else {
            // Host disconnected
            LeaveLobby();
        } 
    }

    void SendLobbyUpdates() {
        foreach (var (id, data) in _playersInLobby) {
            UpdatePlayerClientRpc(id, data);
        }
        for (byte i = 0; i < GameSettings.Properties.Count; i++) {
            var property = GameSettings.Properties[i];
            UpdateLobbySettingsClientRpc(i, property.Value, property.Enabled);
        }
        UpdateLobbyMapClientRpc((byte) LobbyMap.Value);
    }

    [ClientRpc]
    void RemovePlayerClientRpc(ulong player) {
        if (IsServer) return;
        _playersInLobby.Remove(player);
        PlayerRemoved?.Invoke(player);
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
        PlayerUpdatedOrAdded?.Invoke(id, data);
        
        UpdatePlayerClientRpc(id, data);
        Debug.Log($"Set name of player {id} to {playerName}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    void UpdatePlayerServerRpc(ulong playerId, LobbyPlayerData newPlayerData) {
        if (_playersInLobby.ContainsKey(playerId)) {
            // Check if the robot is already taken
            if (_playersInLobby.Where(pair => pair.Key != playerId).Any(pair => pair.Value.RobotId == newPlayerData.RobotId)) {
                newPlayerData.RobotId = _playersInLobby[playerId].RobotId;
            }
        }
        
        _playersInLobby[playerId] = newPlayerData;
        PlayerUpdatedOrAdded?.Invoke(playerId, newPlayerData);
        
        UpdatePlayerClientRpc(playerId, newPlayerData);
    }
    
    [ClientRpc]
    void UpdatePlayerClientRpc(ulong id, LobbyPlayerData data) {
        if (IsServer) return;
        
        _playersInLobby[id] = data;
        PlayerUpdatedOrAdded?.Invoke(id, data);
        Debug.Log($"Updated player {id} with data {data}");
    }

    [ClientRpc]
    void UpdateLobbySettingsClientRpc(byte propertyId, byte value, bool enabled) {
        if (IsServer) return;
        
        var property = GameSettings.Properties[propertyId];
        property.Enabled = enabled;
        property.Value = value;
        LobbySettingsPropertyUpdated?.Invoke(property);
    }

    [ClientRpc]
    void StartGameClientRpc() {
        if (IsServer) return;
        NetworkSystem.CurrentGameType = NetworkSystem.GameType.Multiplayer;
        CanvasHelpers.Instance.ShowOverlay("Game starting...");
    }

    # region Public Methods

    public void RefreshLobbyProperty(GameProperty property) {
        if (!IsServer) return;
        LobbySettingsPropertyUpdated?.Invoke(property);
        var id = GameSettings.Properties.IndexOf(property);
        UpdateLobbySettingsClientRpc((byte) id, property.Value, property.Enabled);
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
        StartGameClientRpc();
        var lockLobbyAsync = Matchmaking.LockLobbyAsync();
        yield return new WaitUntil(() => lockLobbyAsync.IsCompleted);
        
        var sceneManager = NetworkManager.SceneManager;
        var succeeded = false;
        var failed = false;
        
        NetworkSystem.CurrentGameType = NetworkSystem.GameType.Multiplayer;
        
        sceneManager.OnLoadEventCompleted += LoadComplete;
        sceneManager.LoadScene(NetworkSystem.GameScene, LoadSceneMode.Single);
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