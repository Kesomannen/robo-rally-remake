using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

#pragma warning disable 4014

public class LobbySystem : NetworkBehaviour {
    public static LobbySystem Instance { get; private set; }

    Dictionary<ulong, PlayerData> _playersInLobby = new();

    public static event Action<Dictionary<ulong, PlayerData>> OnLobbyChanged;

    void Awake() {
        Instance = this;    
    }

    void Start() {
        NetworkObject.DestroyWithScene = true;

        Matchmaking.InitializeAsync();
    }

    public override void OnDestroy() {
        base.OnDestroy();
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            _playersInLobby.Add(NetworkManager.LocalClientId, new PlayerData());
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnClientConnected(ulong player) {
        if (!IsServer) return;

        _playersInLobby[player] = new();
        SendLobbyUpdates();
    }

    void SendLobbyUpdates() {
        foreach (var player in _playersInLobby) {
            UpdatePlayerClientRpc(player.Key, player.Value);
        }
        RefreshInterface();
    }

    [ClientRpc]
    void UpdatePlayerClientRpc(ulong player, PlayerData data) {
        if (IsServer) return;
        _playersInLobby[player] = data;
        RefreshInterface();
    }

    void OnClientDisconnected(ulong player) {
        if (IsServer) {
            _playersInLobby.Remove(player);
            RemovePlayerClientRpc(player);
            RefreshInterface();
        } else {
            LeaveLobby();
        } 
    }

    [ClientRpc]
    void RemovePlayerClientRpc(ulong player) {
        if (IsServer) return;
        _playersInLobby.Remove(player);
        RefreshInterface();
    }

    void RefreshInterface() {
        OnLobbyChanged?.Invoke(_playersInLobby);
    }

    public async Task LeaveLobby() {
        _playersInLobby.Clear();
        NetworkManager.Shutdown();
        await Matchmaking.LeaveLobbyAsync();
    }

    public struct PlayerData : INetworkSerializable {
        public bool IsReady;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref IsReady);
        }
    }
}