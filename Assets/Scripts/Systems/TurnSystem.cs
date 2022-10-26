using UnityEngine;

public class TurnSystem : Singleton<TurnSystem> {
    public GameEvent OnTurnStart, OnTurnEnd, OnStepStart, OnStepEnd;

    void Start() {
        OnTurnStart.Invoke();
    }
}