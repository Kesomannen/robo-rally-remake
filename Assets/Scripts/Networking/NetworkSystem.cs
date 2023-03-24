using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

#pragma warning disable 4014

public class NetworkSystem : NetworkSingleton<NetworkSystem> {
    [SerializeField] RobotData _singleplayerRobot;

    public static GameType CurrentGameType { get; set; } = GameType.Tutorial;

    public enum GameType {
        Singleplayer,
        Multiplayer,
        Tutorial
    }

    void Start() {
        NetworkObject.DestroyWithScene = true;
        
        if (CurrentGameType == GameType.Multiplayer || NetworkManager.IsListening) return;
        NetworkManager.StartHost();
    }

    public override void OnNetworkSpawn() {
        Debug.Log("NetworkSystem spawned");
        
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;

        if (CurrentGameType == GameType.Multiplayer) {
            if (IsServer) {
                StartCoroutine(WaitForPlayers());
            }
        } else {
            StartGame();
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
                    CanvasHelpers.Instance.ShowError($"{StringUtils.FormatMultiple(clientsCompleted.Count, "player")} timed out");
                }

                var failedToLoad = NetworkManager.ConnectedClientsList.Count - clientsCompleted.Count;
                if (failedToLoad > 0) {
                    Debug.LogError($"Clients did not load: {string.Join(", ", NetworkManager.ConnectedClientsList.Select(c => c.ClientId).Except(clientsCompleted))}");
                    CanvasHelpers.Instance.ShowError($"{StringUtils.FormatMultiple(failedToLoad, "player")} timed out");
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

    void StartGame() {
        if (CurrentGameType == GameType.Tutorial) {
            Tutorial.Instance.Initialize();
        } else {
            List<GameSystem.PlayerData> players = new();

            if (CurrentGameType == GameType.Singleplayer) {
                players.Add(new GameSystem.PlayerData {
                    Id = NetworkManager.Singleton.LocalClientId,
                    Name = LobbySystem.PlayerName,
                    RobotData = _singleplayerRobot
                });
            } else {
                foreach (var (id, playerData) in LobbySystem.PlayersInLobby) {
                    players.Add(new GameSystem.PlayerData {
                        Id = id,
                        Name = playerData.Name,
                        RobotData = RobotData.GetById(playerData.RobotId)
                    });
                }
            }
            
            GameSystem.Initialize(LobbySystem.GameSettings, MapData.GetById(LobbySystem.LobbyMap.Value), players);
        }
        
        Debug.Log("Initialization complete, starting phase system");
        GameSystem.Instance.StartPhaseSystem();
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

    public override void OnDestroy() {
        base.OnDestroy();

        Matchmaking.LeaveLobbyAsync();
        
        if (NetworkManager.Singleton == null) return;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
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
}