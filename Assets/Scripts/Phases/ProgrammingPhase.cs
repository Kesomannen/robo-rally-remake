using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class ProgrammingPhase : NetworkSingleton<ProgrammingPhase> {
    const int _cardsPerTurn = 7;

    static int _playersReady;

    public static IEnumerator DoPhaseRoutine() {
        UIManager.Instance.CurrentState = UIState.Hand;

        _playersReady = 0;
        var orderedPlayers = PlayerManager.GetOrderedPlayers();

        // Draw cards
        foreach (var player in orderedPlayers) {
            if (player == PlayerManager.LocalPlayer) {
                player.DrawCardsUpTo(_cardsPerTurn);
            }
        }

        yield return new WaitUntil(() => _playersReady >= PlayerManager.Players.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LockRegisterServerRpc(byte playerIndex, byte[] registerCardIds) {
        LockPlayerRegister(playerIndex, registerCardIds);
        LockRegisterClientRpc(playerIndex, registerCardIds);
        _playersReady++;
    }

    [ClientRpc]
    void LockRegisterClientRpc(byte playerIndex, byte[] registerCardIds) {
        if (IsServer) return;
        LockPlayerRegister(playerIndex, registerCardIds);
        _playersReady++;
    }

    static void LockPlayerRegister(byte playerIndex, byte[] registerCardIds) {
        if (PlayerManager.Players[playerIndex] == PlayerManager.LocalPlayer) {
            return;
        }

        Debug.Log($"Locking register for player {playerIndex}");

        var player = PlayerManager.Players[playerIndex];
        var cards = registerCardIds.Select(id => ProgramCardData.GetById(id)).ToArray();
        for (int i = 0; i < cards.Length; i++) {
            player.Registers[i] = cards[i];
            Debug.Log($"Register {i} of player {playerIndex} is now {cards[i].Name}");
        }
    }

    public static void Continue() {
        _playersReady = PlayerManager.Players.Count;
    }
}