using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeSystem : Singleton<UpgradeSystem> {
    [SerializeField] float _pauseTime = 2f;

    public static readonly PauseEvent BeforePlayerOrdering = new();
    public static readonly PauseEvent<ProgramCardData> BeforeRegister = new();
    public static readonly PauseEvent<ProgramCardData> AfterRegister = new();

    static int _upgradesUsed;
    
    void Start() {
        foreach (var player in PlayerSystem.Players) {
            player.OnUpgradeUsed += _ => _upgradesUsed++;
        }
    }

    public static IEnumerator AwaitEvent<T>(PauseEvent<T> pauseEvent, T arg) {
        if (!pauseEvent.ShouldAwait(arg)) yield break;
        pauseEvent.Active = true;
        yield return Await();
        pauseEvent.Active = false;
    }
    
    public static IEnumerator AwaitEvent(PauseEvent pauseEvent) {
        if (!pauseEvent.ShouldAwait) yield break;
        pauseEvent.Active = true;
        yield return Await();
        pauseEvent.Active = false;
    }

    static IEnumerator Await() {
        yield return CoroutineUtils.Wait(Instance._pauseTime);
        _upgradesUsed = 0;
    }

    public class PauseEvent<T> {
        readonly List<Func<T, bool>> _listeners = new();
        
        public bool Active;

        static bool True(T arg) => true;

        public bool ShouldAwait(T arg) => _listeners.Any(listener => listener(arg));
        public void AddListener(Func<T, bool> listener = null) => _listeners.Add(listener ?? True);
        public void RemoveListener(Func<T, bool> listener = null) => _listeners.Remove(listener ?? True);
    }

    public class PauseEvent {
        int _listeners;
        
        public bool Active;

        public bool ShouldAwait => _listeners > 0;
        public void AddListener() => _listeners++;
        public void RemoveListener() => _listeners--;
    }
}