using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : Singleton<Scheduler> {
    static List<RoutineItem> _routineList = new();
    public static IReadOnlyList<RoutineItem> RoutineList => _routineList;
    public static RoutineItem? CurrentRoutine { get; private set; } = null;

    static bool _isPlaying;

    public const float DefaultDelay = 0.5f;

    public static void Push(IEnumerator routine, string label, float delay = DefaultDelay) {
        AddItem(new RoutineItem(routine, label, delay), 0);
    }

    public static void Enqueue(IEnumerator routine, string label, float delay = DefaultDelay) {
        AddItem(new RoutineItem(routine, label, delay), _routineList.Count);
    }

    static void AddItem(RoutineItem item, int index) {
        _routineList.Insert(index, item);
        if (!_isPlaying) {
            Instance.StartCoroutine(Instance.PlayStackRoutine());
        }
    }

    public static Coroutine StartRoutine(IEnumerator routine) {
        return Instance.StartCoroutine(routine);
    }

    public static void StopRoutine(Coroutine coroutine) {
        Instance.StopCoroutine(coroutine);
    }

    public static IEnumerator GroupRoutines(IEnumerator[] routines) {
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

    IEnumerator PlayStackRoutine() {
        _isPlaying = true;
        while (_routineList.Count > 0) {
            var routineItem = _routineList[0];
            CurrentRoutine = routineItem;
            _routineList.RemoveAt(0);

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