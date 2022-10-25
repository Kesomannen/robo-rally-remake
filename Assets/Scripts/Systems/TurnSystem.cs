using UnityEngine;

public class TurnSystem : Singleton<TurnSystem> {
    public GameEvent OnTurnStart, OnTurnEnd, OnStepStart, OnStepEnd;

    protected override void Awake() {
        base.Awake();
    }

    void Start() {
        OnTurnStart.Invoke();
    }
}