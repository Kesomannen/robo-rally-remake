using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskScheduler : Singleton<TaskScheduler> {
    static readonly Stack<(IEnumerator Routine, Action Callback, float Delay)> _tasks = new();
    static bool _isRunning;

    const float DefaultTaskDelay = 0.5f;
    
    public static void PushRoutine(IEnumerator routine, float delay = DefaultTaskDelay, Action onComplete = null) {
        _tasks.Push((routine, onComplete, delay));
        Debug.Log($"Pushed routine. {_tasks.Count} tasks in queue.");
        if (_isRunning) return;
        
        _isRunning = true;
        Instance.StartCoroutine(RunTasks());
    }
    
    public static void PushSequence(float delay = DefaultTaskDelay, params IEnumerator[] routines) {
        for (var i = routines.Length - 1; i >= 0; i--){
            _tasks.Push((routines[i], () => {}, delay));
        }
        Debug.Log($"Pushed sequence. {_tasks.Count} tasks in queue.");
        
        if (_isRunning) return;
        
        _isRunning = true;
        Instance.StartCoroutine(RunTasks());
    }
    
    public static void PushSequence(float delay = DefaultTaskDelay, params Action[] actions) {
        for (var i = actions.Length - 1; i >= 0; i--) {
            _tasks.Push((WrapAction(actions[i]), () => {}, delay));
        }
        
        if (_isRunning) return;
        
        _isRunning = true;
        Instance.StartCoroutine(RunTasks());

        IEnumerator WrapAction(Action action) {
            action();
            yield break;
        }
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
        yield return new WaitWhile(() => _isRunning);
    }
}