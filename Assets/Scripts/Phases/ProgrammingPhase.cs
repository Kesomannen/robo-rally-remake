using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class ProgrammingPhase : NetworkSingleton<ProgrammingPhase> {
    public static readonly ObservableField<int> StressTimer = new(0);
    public static bool IsStressed { get; private set; }
    public static bool LocalPlayerSubmitted { get; private set; }

    static int _playersReady;

    public static IEnumerator DoPhaseRoutine() {
        UIManager.Instance.CurrentState = UIState.Hand;

        IsStressed = false;
        LocalPlayerSubmitted = false;
        StressTimer.Value = GameSettings.instance.StressTime;

        var orderedPlayers = PlayerManager.GetOrderedPlayers();

        foreach (var player in orderedPlayers) {
            if (player == PlayerManager.LocalPlayer) {
                player.DrawCardsUpTo(GameSettings.instance.CardsPerTurn);
            }
        }

        _playersReady = 0;
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
        var isLocal = PlayerManager.Players[playerIndex] == PlayerManager.LocalPlayer;
        if (isLocal) {
            LocalPlayerSubmitted = true;
            return;
        }

        Debug.Log($"Locking register for player {playerIndex}");

        var player = PlayerManager.Players[playerIndex];
        var cards = registerCardIds.Select(id => ProgramCardData.GetById(id)).ToArray();
        for (int i = 0; i < cards.Length; i++) {
            player.Program.SetCard(i, cards[i]);
            Debug.Log($"Register {i} of player {playerIndex} is now {cards[i]}");
        }

        if (!LocalPlayerSubmitted && !IsStressed) {
            IsStressed = true;
            Scheduler.StartRoutine(StressRoutine());
        }
    }

    static IEnumerator StressRoutine() {
        while (!LocalPlayerSubmitted) {
            StressTimer.Value--;
            if (StressTimer.Value <= 0) {
                FillRegisters(PlayerManager.LocalPlayer);
                IsStressed = false;
                yield break;
            }
            yield return Helpers.Wait(1);
        }
    }

    static void FillRegisters(Player player) {
        // Discard hand
        for (int i = 0; i < player.Hand.Cards.Count; i++) {
            player.DiscardCard(i);
        }

        // Fill empty registers
        for (int i = 0; i < player.Program.Cards.Count; i++) {
            if (player.Program[i] == null) {
                player.Program.SetCard(i, player.GetTopCard());
            }
        }

        // Lock registers
        player.SerializeRegisters(out var playerIndex, out var registerCardIds);
        Instance.LockRegisterServerRpc(playerIndex, registerCardIds);
    }

    public static void Continue() {
        _playersReady = PlayerManager.Players.Count;
    }
}