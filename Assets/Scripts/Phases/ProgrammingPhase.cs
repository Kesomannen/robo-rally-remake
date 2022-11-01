using System.Collections;
using System;
using UnityEngine;

public class ProgrammingPhase : Phase<ProgrammingPhase> {
    [SerializeField] int _cardsPerTurn;

    static bool _canProceed;

    public override event Action OnPhaseStart, OnPhaseEnd;

    public override IEnumerator DoPhase() {
        OnPhaseStart?.Invoke();

        UIManager.Instance.CurrentState = UIState.Hand;

        _canProceed = false;
        var orderedPlayers = PlayerManager.OrderPlayers();

        // Draw cards
        foreach (var player in orderedPlayers) {
            if (player == PlayerManager.LocalPlayer) {
                player.DrawCardsUpTo(_cardsPerTurn);
            } else {
                player.DrawCardsUpTo(_cardsPerTurn);
            }
        }

        yield return new WaitUntil(() => _canProceed);

        foreach (var player in orderedPlayers) {
            foreach (var register in player.Registers) {
                register.Discard();
            }
        }

        OnPhaseEnd?.Invoke();
    }

    public static void RefreshRegisterState() {
        foreach (var player in PlayerManager.Players) {
            foreach (var register in player.Registers) {
                if (register.IsEmpty) return;
            }
        }
        _canProceed = true;
    }
}