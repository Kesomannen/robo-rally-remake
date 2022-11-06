using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

#pragma warning disable 4014

public class NetworkSystem : NetworkSingleton<NetworkSystem> {
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        MapSystem.Instance.LoadMap(MapData.GetById(LobbySystem.LobbyMapId));

        if (IsServer) {
            var networkPlayers = LobbySystem.PlayersInLobby;
            foreach (var plr in networkPlayers) {
                PlayerManager.Instance.CreatePlayer(plr.Key, plr.Value);
                CreatePlayerClientRpc(plr.Key, plr.Value);
            }
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();

        Matchmaking.LeaveLobbyAsync();
        if (NetworkManager.Singleton != null) {
            NetworkManager.Shutdown();
        }
    }

    [ClientRpc]
    void CreatePlayerClientRpc(ulong id, PlayerData data) {
        if (IsServer) return;
        PlayerManager.Instance.CreatePlayer(id, data);
    }
}