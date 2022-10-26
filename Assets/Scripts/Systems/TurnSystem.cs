using System.Collections;
using System;

public class TurnSystem : Singleton<TurnSystem> {
    public static int CurrentTurn { get; private set; } = -1;
    public static int CurrentStep { get; private set; }
    public static int CurrentPlayerIndex { get; private set; }

    public static Player CurrentPlayer => _playersInOrder[CurrentPlayerIndex];

    public const int StepsPerTurn = 5;

    static Player[] _playersInOrder;

    public event Action<int> OnTurnStart, OnTurnEnd, OnStepStart, OnStepEnd;

    protected override void Awake() {
        base.Awake();
    }

    void Start() {
        StartTurn();
    }

    void StartTurn() {
        CurrentTurn++;
        CurrentStep = -1;

        OnTurnStart?.Invoke(CurrentTurn);
        StartStep();
    }

    void EndTurn() {
        OnTurnEnd?.Invoke(CurrentTurn);
    }

    void StartStep() {
        CurrentStep++;
        OnStepStart?.Invoke(CurrentStep);
        StartCoroutine(ExecuteRegisters());
    }

    void EndStep() {
        OnStepEnd?.Invoke(CurrentStep);
        if (CurrentStep >= StepsPerTurn - 1) {
            EndTurn();
        } else {
            StartStep();
        }
    }

    IEnumerator ExecuteRegisters() {
        foreach (var player in _playersInOrder) {
            player.Registers[CurrentStep].Execute(player, CurrentStep);
            yield return Scheduler.WaitUntilQueueEmpty();
        }
        EndStep();
    }

    void OrderPlayers() {
        _playersInOrder = PlayerManager.Players;
    }
}