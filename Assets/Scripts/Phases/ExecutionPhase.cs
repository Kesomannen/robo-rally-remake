using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExecutionPhase : NetworkSingleton<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }

    public const int RegisterCount = 5;
    const float RegisterDelay = 1f;
    const float SubPhaseDelay = 0.5f;

    public static event Action OnPhaseStart, OnPhaseEnd, OnExecutionComplete;
    public static event Action<ProgramCardData, int, Player> BeforeRegister, AfterRegister;
    public static event Action<ExecutionSubPhase> OnNewSubPhase;
    public static event Action<IReadOnlyList<Player>> OnPlayersOrdered;

    public static IEnumerator DoPhaseRoutine() {
        OnPhaseStart?.Invoke();
        UIManager.Instance.ChangeState(UIState.Map);
        
        for (CurrentRegister = 0; CurrentRegister < RegisterCount; CurrentRegister++){
            var orderedPlayers = PlayerManager.GetOrderedPlayers().ToArray();
            OnPlayersOrdered?.Invoke(orderedPlayers);

            yield return DoSubPhase(ExecutionSubPhase.Registers, ExecuteRegister(orderedPlayers));
            yield return DoSubPhase(ExecutionSubPhase.Conveyor, Conveyor.ActivateElement());
            yield return DoSubPhase(ExecutionSubPhase.PushPanel, PushPanel.ActivateElement());
            yield return DoSubPhase(ExecutionSubPhase.Gear, Gear.ActivateElement());
            yield return DoSubPhase(ExecutionSubPhase.BoardLaser, BoardLaser.ActivateElement());
            yield return DoSubPhase(ExecutionSubPhase.PlayerLaser, FireLasers(orderedPlayers));
            yield return DoSubPhase(ExecutionSubPhase.EnergySpace, EnergySpace.ActivateElement());
            yield return DoSubPhase(ExecutionSubPhase.Checkpoint, Checkpoint.ActivateElement());
        }

        OnExecutionComplete?.Invoke();

        foreach (var player in PlayerManager.Players) {
            player.DiscardProgram();
        }

        OnPhaseEnd?.Invoke();

        IEnumerator DoSubPhase(ExecutionSubPhase subPhase, IEnumerator routine) {
            OnNewSubPhase?.Invoke(subPhase);
            //yield return Helpers.Wait(SubPhaseDelay);
            yield return routine;
            yield return Helpers.Wait(0.5f);
        }
    }

    static IEnumerator ExecuteRegister(IEnumerable<Player> players) {
        foreach (var player in players) {
            var card = player.Program[CurrentRegister];
            if (card == null) continue;

            BeforeRegister?.Invoke(card, CurrentRegister, player);
            
            Scheduler.Enqueue(card.ExecuteRoutine(player, CurrentRegister), $"ProgramCard ({card})", RegisterDelay);
            yield return Scheduler.WaitUntilClearRoutine();
            
            AfterRegister?.Invoke(card, CurrentRegister, player);
        }
    }

    static IEnumerator FireLasers(IEnumerable<Player> players) {
        return players
            .Where(p => !p.IsRebooted.Value)
            .Select(p => p.Model.FireLaser(p.Model.Rotator.Identity))
            .GetEnumerator();
    }
}

public enum ExecutionSubPhase {
    Registers,
    Conveyor,
    PushPanel,
    Gear,
    BoardLaser,
    PlayerLaser,
    EnergySpace,
    Checkpoint,
}