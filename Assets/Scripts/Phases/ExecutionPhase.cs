using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.UI;

#pragma warning disable 0067

public class ExecutionPhase : NetworkSingleton<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }

    public const int RegisterCount = 5;

    public static event Action OnPhaseEnd, OnExecutionComplete;

    public static IEnumerator DoPhaseRoutine() {
        UIManager.Instance.ChangeState(UIState.Map);
        
        for (CurrentRegister = 0; CurrentRegister < RegisterCount; CurrentRegister++){
            var orderedPlayers = PlayerManager.GetOrderedPlayers().ToArray();
            yield return ExecuteRegister(orderedPlayers);
            
            yield return Conveyor.ActivateElement();
            yield return PushPanel.ActivateElement();
            yield return Gear.ActivateElement();
            yield return BoardLaser.ActivateElement();
            yield return FireLasers(orderedPlayers);
            yield return EnergySpace.ActivateElement();
            yield return Checkpoint.ActivateElement();
        }

        OnExecutionComplete?.Invoke();

        foreach (var player in PlayerManager.Players) {
            player.DiscardProgram();
        }

        OnPhaseEnd?.Invoke();
    }

    static IEnumerator ExecuteRegister(IEnumerable<Player> players) {
        foreach (var player in players) {
            var card = player.Program[CurrentRegister];
            if (card == null) continue;

            Scheduler.Enqueue(card.ExecuteRoutine(player, CurrentRegister), $"ProgramCard ({card})");
        }
        yield return Scheduler.WaitUntilClearRoutine();
    }

    static IEnumerator FireLasers(IEnumerable<Player> players) {
        foreach (var player in players){
            if (player.IsRebooted) continue;
            var model = player.Model;
            yield return model.FireLaser(model.Rotator.Identity);
        }
    }
}