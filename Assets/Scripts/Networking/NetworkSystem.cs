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