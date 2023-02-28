using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            PlayerSystem.Instance.CreatePlayer(id, data, false);
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

    static readonly List<ProgramCardData> _queryResults = new();
    static bool _querying;

    [ClientRpc]
    void QueryClientRpc(byte playerIndex, byte pile, byte startIndex, byte endIndex) {
        if (PlayerSystem.Players[playerIndex] != PlayerSystem.LocalPlayer) return;
        Debug.Log($"Querying {PlayerSystem.Players[playerIndex]}'s {(Pile)pile} pile from {startIndex} to {endIndex}");
        
        var depth = endIndex - startIndex;
        var collection = PlayerSystem.LocalPlayer.GetCollection((Pile)pile);
        if (collection.Cards.Count < startIndex + depth) {
            if ((Pile)pile == Pile.DrawPile) {
                PlayerSystem.LocalPlayer.ShuffleDeck();
            } else {
                Debug.LogError($"Cannot query {PlayerSystem.LocalPlayer}'s {(Pile)pile} pile from {startIndex} to {endIndex}; not enough cards");
                return;
            }
        }

        var result = collection
            .Cards.Skip(startIndex)
            .Take(depth)
            .Select(c => (byte)c.GetLookupId())
            .ToArray();
        
        SendQueryResultsServerRpc(result);
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SendQueryResultsServerRpc(byte[] cardIds) {
        _queryResults.AddRange(cardIds.Select(c => ProgramCardData.GetById(c)));
        SendQueryResultsClientRpc(cardIds);
    }
    
    [ClientRpc]
    void SendQueryResultsClientRpc(byte[] cardIds) {
        if (IsServer) return;
        _queryResults.AddRange(cardIds.Select(c => ProgramCardData.GetById(c)));
        Debug.Log($"Received query results: {string.Join(", ", _queryResults.Select(c => c.Name))}");
    }

    public IEnumerator QueryPlayerCards(Player player, Pile pile, int startIndex, int endIndex, List<ProgramCardData> result) {
        if (_querying) throw new InvalidOperationException("Cannot query while another query is in progress.");
        _querying = true;
        
        if (IsServer) {
            var playerIndex = (byte)PlayerSystem.Players.IndexOf(player);
            QueryClientRpc(playerIndex, (byte) pile, (byte) startIndex, (byte) endIndex);
        }
        while (_queryResults.Count == 0) {
            yield return null;
        }
        result.AddRange(_queryResults);
        _queryResults.Clear();
        _querying = false;
    }
}