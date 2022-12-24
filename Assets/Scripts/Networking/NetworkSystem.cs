using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

#pragma warning disable 4014

public class NetworkSystem : NetworkSingleton<NetworkSystem> {
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        
        Debug.Log("NetworkSystem spawned, loading map...");
        MapSystem.Instance.LoadMap(MapData.GetById(LobbySystem.LobbyMapId));
        
        foreach (var (id, data) in LobbySystem.PlayersInLobby) {
            PlayerSystem.Instance.CreatePlayer(id, data);
        }
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
}