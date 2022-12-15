using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

#pragma warning disable 4014

public class NetworkSystem : NetworkSingleton<NetworkSystem> {
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        
        Debug.Log("NetworkSystem spawned, loading map...");
        MapSystem.Instance.LoadMap(MapData.GetById(LobbySystem.LobbyMapId));

        if (!IsServer) return;
        
        var networkPlayers = LobbySystem.PlayersInLobby;
        foreach (var plr in networkPlayers) {
            PlayerManager.Instance.CreatePlayer(plr.Key, plr.Value);
            CreatePlayerClientRpc(plr.Key, plr.Value);
        }
    }

    void Start() {
        PhaseSystem.StartPhaseSystem();
    }

    public override void OnDestroy() {
        base.OnDestroy();

        Matchmaking.LeaveLobbyAsync();
        if (NetworkManager.Singleton != null) {
            NetworkManager.Shutdown();
        }
    }

    const int LobbySceneIndex = 0;
    
    public static void ReturnToLobby() {
        SceneManager.LoadScene(LobbySceneIndex);
    }

    [ClientRpc]
    void CreatePlayerClientRpc(ulong id, LobbyPlayerData data) {
        if (IsServer) return;
        PlayerManager.Instance.CreatePlayer(id, data);
    }
}