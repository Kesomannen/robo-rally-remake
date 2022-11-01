using UnityEngine;
using Unity.Netcode;
using System.Linq;

#pragma warning disable 4014

public class NetworkSystem : NetworkBehaviour {
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        if (IsServer) {
            var playerIds = NetworkManager.ConnectedClientsIds.ToArray();
            PlayerManager.Instance.CreatePlayers(playerIds);
            CreatePlayersClientRpc(playerIds);
        }
    }

    [ClientRpc]
    void CreatePlayersClientRpc(ulong[] playerIds) {
        if (IsServer) return;
        PlayerManager.Instance.CreatePlayers(playerIds);
    }

    public override void OnDestroy() {
        base.OnDestroy();

        Matchmaking.LeaveLobbyAsync();
        if (NetworkManager.Singleton != null) {
            NetworkManager.Shutdown();
        }
    }
}