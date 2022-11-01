using UnityEngine;
using Unity.Netcode;

# pragma warning disable 4014

public class NetworkSystem : NetworkBehaviour {
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
    }

    public override void OnDestroy() {
        base.OnDestroy();

        Matchmaking.LeaveLobbyAsync();
        if (NetworkManager.Singleton != null) {
            NetworkManager.Shutdown();
        }
    }
}