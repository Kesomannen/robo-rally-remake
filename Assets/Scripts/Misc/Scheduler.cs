using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : Singleton<Scheduler> {
    static readonly Stack<ScheduleItem> _routineList = new();
    public static IEnumerable<ScheduleItem> RoutineList => _routineList;
    public static ScheduleItem? CurrentRoutine { get; private set; } = null;

    static bool _isPlaying;

    const float DefaultDelay = 0.5f;

    public static void Push(IEnumerator routine, string label, float delay = DefaultDelay) {
        PushItem(new ScheduleItem(routine, label, delay));
    }

    static void PushItem(ScheduleItem item) {
        _routineList.Push(item);
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
            var routineItem = _routineList.Peek();
            CurrentRoutine = routineItem;

            yield return routineItem.Routine;
            yield return CoroutineUtils.Wait(routineItem.Delay);

            _routineList.Pop();
        }
        CurrentRoutine = null;
        _isPlaying = false;
    }

    public struct ScheduleItem {
        public readonly IEnumerator Routine;
        public readonly string Label;
        public readonly float Delay;

        public ScheduleItem(IEnumerator routine, string label, float delay) {
            Routine = routine;
            Label = label;
            Delay = delay;
        }
    }
}