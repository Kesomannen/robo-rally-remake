using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskScheduler : Singleton<TaskScheduler> {
    [SerializeField] [ReadOnly] float _defaultTaskDelay;
    
    static readonly Stack<(IEnumerator Routine, Action Callback, float Delay)> _tasks = new();
    static bool _isRunning;
    
    public static int TaskCount => _tasks.Count;

    public static float DefaultTaskDelay { get; private set; }

    void Start() {
        DefaultTaskDelay = 0.75f / GameSystem.Settings.GameSpeed.Value;
        _defaultTaskDelay = DefaultTaskDelay;
    }

    public static void PushRoutine(IEnumerator routine, float delay = -1, Action onComplete = null) {
        if (delay < 0) delay = DefaultTaskDelay;
        
        _tasks.Push((routine, onComplete, delay));
        if (_isRunning) return;
        
        _isRunning = true;
        Instance.StartCoroutine(RunTasks());
    }
    
    public static void PushSequence(float delay = -1, params IEnumerator[] routines) {
        if (delay < 0) delay = DefaultTaskDelay;
        
        for (var i = routines.Length - 1; i >= 0; i--){
            _tasks.Push((routines[i], () => {}, delay));
        }

        if (_isRunning) return;
        
        _isRunning = true;
        Instance.StartCoroutine(RunTasks());
    }

    static IEnumerator RunTasks() {
        while (_tasks.Count > 0){
            var task = _tasks.Pop();
            yield return task.Routine;
            task.Callback?.Invoke();
            yield return CoroutineUtils.Wait(task.Delay);
        }
        _isRunning = false;
    }
    
    public static IEnumerator WaitUntilClear() {
        yield return new WaitWhile(() => _isRunning || _tasks.Count > 0);
    }
}