using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkUtils : NetworkSingleton<NetworkUtils> {
    # region Querying
    
    readonly Queue<ProgramCardData[]> _queryResults = new();
    bool _querying;

    [ClientRpc]
    void QueryClientRpc(byte playerIndex, byte pile, byte startIndex, byte endIndex) {
        var localPlayer = PlayerSystem.LocalPlayer;
        if (PlayerSystem.Players[playerIndex] != localPlayer) return;
        Debug.Log($"Querying {PlayerSystem.Players[playerIndex]}'s {(Pile)pile} pile from {startIndex} to {endIndex}");
        
        if (!GetQuery((Pile) pile, startIndex, endIndex, localPlayer, out var result)) {
            return;
        }

        SendQueryResultsServerRpc(result);
    }
    
    static bool GetQuery(Pile pile, int startIndex, int endIndex, Player player, out byte[] result) {
        var depth = endIndex - startIndex;
        var collection = player.GetCollection(pile);
        
        if (collection.Cards.Count <= startIndex + depth) {
            if (pile == Pile.DrawPile) {
                player.ShuffleDeck();
            } else {
                Debug.LogError($"Cannot query {player}'s {pile} pile from {startIndex} to {endIndex}; not enough cards");
                result = null;
                return false;
            }
        }

        result = collection.Cards
            .Skip(startIndex)
            .Take(depth)
            .Select(c => (byte)c.GetLookupId())
            .ToArray();
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    void SendQueryResultsServerRpc(byte[] cardIds) {
        var cards = cardIds.Select(c => ProgramCardData.GetById(c)).ToArray();
        _queryResults.Enqueue(cards);
        SendQueryResultsClientRpc(cardIds);
    }
    
    [ClientRpc]
    void SendQueryResultsClientRpc(byte[] cardIds) {
        if (IsServer) return;
        var cards = cardIds.Select(c => ProgramCardData.GetById(c)).ToArray();
        _queryResults.Enqueue(cards);
    }

    public IEnumerator QueryPlayerCards(Player player, Pile pile, int startIndex, int endIndex, List<ProgramCardData> result) {
        if (_querying) throw new InvalidOperationException("Cannot query while another query is in progress.");
        _querying = true;
        
        if (IsServer) {
            var playerIndex = PlayerSystem.Players.IndexOf(player);
            QueryClientRpc((byte) playerIndex, (byte) pile, (byte) startIndex, (byte) endIndex);
        }
        yield return new WaitUntil(() => _queryResults.Count > 0);
        
        result.AddRange(_queryResults.Dequeue());
        _querying = false;
    }
    
    #endregion
    
    #region Syncing

    readonly List<ulong> _playersReady = new();

    public IEnumerator SyncPlayers() {
        if (PlayerSystem.Players.Count <= 1) yield break;

        using (new LoadingScreen("Waiting for players... ({0/0})")) {
            PlayerReadyServerRpc(NetworkManager.LocalClientId);
            while (_playersReady.Count < PlayerSystem.Players.Count) {
                CanvasHelpers.Instance.SetOverlayText($"Waiting for players... ({_playersReady.Count}/{PlayerSystem.Players.Count})");
                yield return null;
            }
        }
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

    #endregion
}