using System.Collections;
using System;
using Unity.Netcode;

public abstract class Phase : NetworkBehaviour {
    public abstract event Action OnPhaseStart, OnPhaseEnd;
    public abstract IEnumerator DoPhase();

    public bool IsRunning { get; private set; }

    protected virtual void Awake() {
        OnPhaseStart += () => IsRunning = true;
        OnPhaseEnd += () => IsRunning = false;
    }
}