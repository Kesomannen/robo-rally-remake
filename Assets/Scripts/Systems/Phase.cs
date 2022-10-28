using System.Collections;
using System;

public abstract class Phase<T> : Singleton<T> where T : Phase<T> {
    public abstract event Action OnPhaseStart, OnPhaseEnd;
    public abstract IEnumerator DoPhase();

    public bool IsRunning { get; private set; }

    protected override void Awake() {
        base.Awake();
        OnPhaseStart += () => IsRunning = true;
        OnPhaseEnd += () => IsRunning = false;
    }
}