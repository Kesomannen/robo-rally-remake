using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : Singleton<Scheduler> {
    static readonly List<RoutineItem> _routineList = new();
    public static IEnumerable<RoutineItem> RoutineList => _routineList;
    public static RoutineItem? CurrentRoutine { get; private set; } = null;

    static bool _isPlaying;

    const float DefaultDelay = 0.5f;

    public static void Push(IEnumerator routine, string label, float delay = DefaultDelay) {
        AddItem(new RoutineItem(routine, label, delay), _routineList.Count);
    }

    public static void Enqueue(IEnumerator routine, string label, float delay = DefaultDelay) {
        AddItem(new RoutineItem(routine, label, delay), 0);
    }

    static void AddItem(RoutineItem item, int index) {
        _routineList.Insert(index, item);
        if (!_isPlaying) {
            Instance.StartCoroutine(PlayListRoutine());
        }
    }

    public static Coroutine StartRoutine(IEnumerator routine) {
        return Instance.StartCoroutine(routine);
    }

    public static void StopRoutine(Coroutine coroutine) {
        Instance.StopCoroutine(coroutine);
    }

    public static IEnumerator GroupRoutines(params IEnumerator[] routines) {
        var routinesInProgress = 0;
        foreach (var routine in routines) {
            routinesInProgress++;
            StartRoutine(WrapRoutine(routine));
        }
        yield return new WaitUntil(() => routinesInProgress <= 0);

        IEnumerator WrapRoutine(IEnumerator routine) {
            yield return routine;
            routinesInProgress--;
        }
    }

    public static IEnumerator WaitUntilClearRoutine() {
        yield return new WaitUntil(() => !_isPlaying);
    }

    static IEnumerator PlayListRoutine() {
        _isPlaying = true;
        while (_routineList.Count > 0) {
            var i = _routineList.Count - 1;
            var routineItem = _routineList[i];
            CurrentRoutine = routineItem;
            _routineList.RemoveAt(i);

            yield return routineItem.Routine;
            yield return Helpers.Wait(routineItem.Delay);
        }
        CurrentRoutine = null;
        _isPlaying = false;
    }

    public struct RoutineItem {
        public IEnumerator Routine;
        public string Label;
        public float Delay;

        public RoutineItem(IEnumerator routine, string label, float delay) {
            Routine = routine;
            Label = label;
            Delay = delay;
        }
    }
}