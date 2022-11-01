using System.Collections;
using System;

public class ExecutionPhase : Phase<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }
    public static GamePlayer CurrentPlayer { get; private set; }

    public const int RegisterCount = 5;

    public event Action<int> OnRegisterStart, OnRegisterEnd;
    public override event Action OnPhaseStart, OnPhaseEnd;

    public override IEnumerator DoPhase() {
        OnPhaseStart?.Invoke();
        UIManager.Instance.CurrentState = UIState.Map;
        for (CurrentRegister = 0; CurrentRegister < RegisterCount; CurrentRegister++) {
            foreach (var player in PlayerManager.OrderPlayers()) {
                CurrentPlayer = player;
                var routine = player.Registers[CurrentRegister].Card.Data.Execute(player, CurrentRegister);
                yield return Scheduler.AddItemAndWait(new ScheduleRoutine(routine));
            }
            yield return Conveyor.Activate();
            OnRegisterEnd?.Invoke(CurrentRegister);
        }
        OnPhaseEnd?.Invoke();
    }
}