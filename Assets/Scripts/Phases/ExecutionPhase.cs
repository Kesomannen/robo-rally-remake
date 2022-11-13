using System;
using System.Collections;
using UnityEngine.Rendering.UI;

#pragma warning disable 0067

public class ExecutionPhase : NetworkSingleton<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }

    public const int RegisterCount = 5;

    public static event Action OnPhaseEnd, OnExecutionComplete;

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

        OnExecutionComplete?.Invoke();

        foreach (var player in PlayerManager.Players) {
            player.DiscardProgram();
        }

        OnPhaseEnd?.Invoke();
    }

    static IEnumerator ExecuteRegister() {
        var orderedPlayers = PlayerManager.GetOrderedPlayers();
        foreach (var player in orderedPlayers) {
            var card = player.Program[CurrentRegister];
            if (card == null) continue;

            Scheduler.Enqueue(card.ExecuteRoutine(player, CurrentRegister), $"ProgramCard ({card})");
        }
        yield return Scheduler.WaitUntilClearRoutine();
    }
}