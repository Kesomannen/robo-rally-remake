using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExecutionPhase : NetworkSingleton<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }

    public const int RegisterCount = 5;
    const float SubPhaseDelay = 0;
    const float StepDelay = 0.5f;

    public static event Action OnPhaseStart, OnPhaseEnd, OnExecutionComplete, OnPlayerRegistersComplete;
    public static event Action<ExecutionSubPhase> OnNewSubPhase;
    public static event Action<IReadOnlyList<Player>> OnPlayersOrdered;
    public static event Action<int> OnNewRegister;

    public static event Action<ProgramExecution> OnPlayerRegister;

    public static IEnumerable<Action> GetPhaseEndInvocations() {
        var invocationList = OnPhaseEnd?.GetInvocationList();
        return invocationList?.Cast<Action>();
    }

    public static IEnumerator DoPhase() {
        yield return UIManager.Instance.ChangeState(UIState.Execution);
        OnPhaseStart?.Invoke();
        
        TaskScheduler.PushSequence(
            delay: StepDelay,
            routines: EnumerableUtils.For(RegisterCount, DoRegister).ToArray()
            );

        yield return TaskScheduler.WaitUntilClear();
        
        OnExecutionComplete?.Invoke();
        
        foreach (var player in PlayerSystem.Players) {
            player.DiscardProgram();
        }
        
        yield return TaskScheduler.WaitUntilClear();

        OnPhaseEnd?.Invoke();
    }

    static Player[] _previousPlayerOrder;
    
    static IEnumerator DoRegister(int register) {
        Debug.Log($"Starting register {register}");
        
        CurrentRegister = register;
        OnNewRegister?.Invoke(register);

        yield return CoroutineUtils.Wait(StepDelay);
        yield return UpgradeAwaiter.AwaitEvent(UpgradeAwaiter.BeforePlayerOrdering);

        var orderedPlayers = PlayerSystem.GetOrderedPlayers().ToArray();
        if (_previousPlayerOrder != null && !_previousPlayerOrder.SequenceEqual(orderedPlayers)) {
            OnPlayersOrdered?.Invoke(orderedPlayers);
            yield return CoroutineUtils.Wait(StepDelay);
        }
        _previousPlayerOrder = orderedPlayers;
        
        var registerRoutines = new IEnumerator[orderedPlayers.Length];
        for (var i = 0; i < orderedPlayers.Length; i++) {
            registerRoutines[i] = DoPlayerRegister(orderedPlayers[i]);
        }

        TaskScheduler.PushSequence(
            delay: SubPhaseDelay,
            DoSubPhase(ExecutionSubPhase.Registers, () => {
                TaskScheduler.PushSequence(routines: registerRoutines);
                return PlayerSystem.Players.Any(p => !p.IsRebooted.Value);
            }),
            CompletePlayerRegisters(),
            DoSubPhase(ExecutionSubPhase.Conveyor, Conveyor.ActivateElement),
            DoSubPhase(ExecutionSubPhase.PushPanel, PushPanel.ActivateElement),
            DoSubPhase(ExecutionSubPhase.Gear, Gear.ActivateElement),
            DoSubPhase(ExecutionSubPhase.BoardLaser, BoardLaser.ActivateElement),
            DoSubPhase(ExecutionSubPhase.PlayerLaser, PlayerModel.ShootLasers),
            DoSubPhase(ExecutionSubPhase.EnergySpace, EnergySpace.ActivateElement),
            DoSubPhase(ExecutionSubPhase.Checkpoint, Checkpoint.ActivateElement)
        );

        IEnumerator CompletePlayerRegisters() {
            OnPlayerRegistersComplete?.Invoke();
            yield break;
        }
        
        IEnumerator DoSubPhase(ExecutionSubPhase subPhase, Func<bool> execute) {
            if (execute()) {
                OnNewSubPhase?.Invoke(subPhase);
                yield return CoroutineUtils.Wait(SubPhaseDelay);
            }
        }

        IEnumerator DoPlayerRegister(Player player) {
            Debug.Log($"Starting player {player} register {register}");
            
            yield return UpgradeAwaiter.AwaitEvent(UpgradeAwaiter.BeforeRegister, player);
            if (player.Program[register] == null) yield break;
            var execution = new ProgramExecution(player.Program[register], player, register);
            
            TaskScheduler.PushRoutine(AfterRegister(execution));
            TaskScheduler.PushRoutine(execution.Execute());
            OnPlayerRegister?.Invoke(execution);
        }

        IEnumerator AfterRegister(ProgramExecution execution) {
            yield return UpgradeAwaiter.AwaitEvent(UpgradeAwaiter.AfterRegister, execution.Player);
        }
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