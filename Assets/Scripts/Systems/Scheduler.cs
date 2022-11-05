using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : Singleton<Scheduler> {
    static Stack<RoutineItem> _routineStack = new();
    static bool _isPlaying;

    public const float DefaultDelay = 0.5f;

    public static void AddRoutine(IEnumerator routine, float delay = DefaultDelay) {
        _routineStack.Push(new RoutineItem(routine, delay));
        if (!_isPlaying) {
            Instance.StartCoroutine(Instance.PlayStackRoutine());
        }
    }

    public static Coroutine StartRoutine(IEnumerator routine) {
        return Instance.StartCoroutine(routine);
    }

    public static IEnumerator PlayListRoutine(IEnumerable<IEnumerator> routines) {
        var routinesInProgress = 0;
        foreach (var routine in routines) {
            routinesInProgress++;
            StartRoutine(RunRoutine(routine));
        }
        yield return new WaitUntil(() => routinesInProgress == 0);

        IEnumerator RunRoutine(IEnumerator routine) {
            yield return routine;
            routinesInProgress--;
        }
    }

    public static IEnumerator WaitUntilClearRoutine() {
        yield return new WaitUntil(() => !_isPlaying);
    }

    IEnumerator PlayStackRoutine() {
        _isPlaying = true;
        while (_routineStack.Count > 0) {
            var routineItem = _routineStack.Pop();
            yield return routineItem.Routine;
            yield return Helpers.Wait(routineItem.Delay);
        }
        _isPlaying = false;
    }

    struct RoutineItem {
        public IEnumerator Routine;
        public float Delay;

        public RoutineItem(IEnumerator routine, float delay) {
            Routine = routine;
            Delay = delay;
        }
    }
}