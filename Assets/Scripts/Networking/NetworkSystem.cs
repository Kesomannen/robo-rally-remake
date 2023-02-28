using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

#pragma warning disable 4014

public class NetworkSystem : NetworkSingleton<NetworkSystem> {
    [SerializeField] GameObject _waitingOverlay;
    [SerializeField] TMP_Text _waitingText;

    void Start() {
        NetworkObject.DestroyWithScene = true;
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        
        MapSystem.Instance.LoadMap(MapData.GetById(LobbySystem.LobbyMap.Value));
        
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
    
    readonly List<ulong> _playersReady = new();

    public IEnumerator SyncPlayers() {
        if (NetworkManager == null) yield break;
        
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
}