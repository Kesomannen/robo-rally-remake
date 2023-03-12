using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeAwaiter : Singleton<UpgradeAwaiter> {
    [SerializeField] float _pauseTime = 2f;

    public static readonly PauseEvent BeforePlayerOrdering = new();
    public static readonly PauseEvent<Player> BeforeRegister = new();
    public static readonly PauseEvent<Player> AfterRegister = new();

    public static event Action OnPauseEventStart, OnPauseEventEnd;

    public static IEnumerator AwaitEvent<T>(PauseEvent<T> pauseEvent, T arg) {
        if (!pauseEvent.ShouldAwait(arg)) yield break;
        pauseEvent.Activate(arg);
        yield return Await();
        pauseEvent.Deactivate();
    }
    
    public static IEnumerator AwaitEvent(PauseEvent pauseEvent) {
        if (!pauseEvent.ShouldAwait) yield break;
        pauseEvent.Active = true;
        yield return Await();
        pauseEvent.Active = false;
    }

    static IEnumerator Await() {
        var duration = 0f;
        var startTasks = TaskScheduler.TaskCount;
        OnPauseEventStart?.Invoke();

        while (duration < Instance._pauseTime && TaskScheduler.TaskCount == startTasks) {
            duration += Time.deltaTime;
            yield return null;
        }
        
        OnPauseEventEnd?.Invoke();
    }

    public class PauseEvent<T> {
        readonly List<T> _listeners = new();
        T _currentArg;
        bool _active;
        
        public bool ActiveFor(T arg) => _active && _currentArg.Equals(arg);
        public void Activate(T arg) {
            _currentArg = arg;
            _active = true;
        }
        public void Deactivate() => _active = false;
        
        public bool ShouldAwait(T arg) => _listeners.Any(listener => listener.Equals(arg));
        public void AddListener(T listener) => _listeners.Add(listener);
        public void RemoveListener(T listener) => _listeners.Remove(listener);
    }

    public class PauseEvent {
        int _listeners;

        public bool Active;

        public bool ShouldAwait => _listeners > 0;
        public void AddListener() => _listeners++;
        public void RemoveListener() => _listeners--;
    }
}