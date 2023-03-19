using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeAwaiter : Singleton<UpgradeAwaiter> {
    [SerializeField] float _pauseTime = 2f;

    public static readonly PauseEvent BeforePlayerOrdering = new();
    public static readonly PlayerPauseEvent BeforeRegister = new();
    public static readonly PlayerPauseEvent AfterRegister = new();

    public static event Action OnPauseEventStart, OnPauseEventEnd;

    public static IEnumerator AwaitEvent(PlayerPauseEvent pauseEvent, Player arg) {
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

    public class PlayerPauseEvent {
        readonly List<Player> _listeners = new();
        Player _current;
        bool _active;
        
        public bool ActiveFor(Player arg) => _active && _current == arg && !arg.IsRebooted.Value;
        public void Activate(Player arg) {
            _current = arg;
            _active = true;
        }
        public void Deactivate() => _active = false;

        public bool ShouldAwait(Player arg) => _listeners.Any(listener => listener == arg && !listener.IsRebooted.Value);
        public void AddListener(Player listener) => _listeners.Add(listener);
        public void RemoveListener(Player listener) => _listeners.Remove(listener);
    }

    public class PauseEvent {
        int _listeners;

        public bool Active;

        public bool ShouldAwait => _listeners > 0;
        public void AddListener() => _listeners++;
        public void RemoveListener() => _listeners--;
    }
}