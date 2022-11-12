using System;
using System.Collections;
using UnityEngine.Rendering.UI;

#pragma warning disable 0067

public class ExecutionPhase : NetworkSingleton<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }

    public const int RegisterCount = 5;

    public static event Action OnPhaseEnd;

    public static IEnumerator DoPhaseRoutine() {
        UIManager.Instance.CurrentState = UIState.Map;

        // Execute registers
        for (CurrentRegister = 0; CurrentRegister < RegisterCount; CurrentRegister++) {
            yield return ExecuteRegister();

            // Board elements
            yield return Conveyor.ActivateRoutine();
            yield return PushPanel.ActivateRoutine();
            yield return Gear.ActivateRoutine();
            yield return EnergySpace.ActivateRoutine();
            yield return Checkpoint.ActivateRoutine();
        }

        DiscardRegisters();

        OnPhaseEnd?.Invoke();
    }

    static IEnumerator ExecuteRegister() {
        var orderedPlayers = PlayerManager.GetOrderedPlayers();
        foreach (var player in orderedPlayers) {
            var card = player.Program[CurrentRegister];
            if (card == null) continue;

            Scheduler.Enqueue(card.ExecuteRoutine(player, CurrentRegister));
        }
        yield return Scheduler.WaitUntilClearRoutine();
    }

    static void DiscardRegisters() {
        foreach (var player in PlayerManager.Players) {
            for (int i = 0; i < player.Program.Cards.Count; i++) {
                var card = player.Program[i];
                if (card == null) continue;

                player.DiscardPile.AddCard(card, CardPlacement.Top);
                player.Program.SetCard(i, null);
            }
        }
    }
}