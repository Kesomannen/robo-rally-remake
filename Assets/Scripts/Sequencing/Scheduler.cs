using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : Singleton<Scheduler> {
    static Queue<IScheduleItem> _routines = new();
    static bool _isPlaying;

    public const float DefaultDelay = 0.5f;
    public const float DefaultInterval = 0.25f;

    public static void AddItem(IScheduleItem item) {
        _routines.Enqueue(item);
        if (!_isPlaying) {
            instance.StartCoroutine(instance.PlayItems());
        }
    }

    public static Coroutine StartRoutine(IEnumerator routine) {
        return instance.StartCoroutine(routine);
    }

    public static IEnumerator WaitUntilQueueEmpty() {
        yield return new WaitWhile(() => _routines.Count > 0);
    }

    IEnumerator PlayItems() {
        _isPlaying = true;
        while (_routines.Count > 0) {
            yield return _routines.Dequeue().Play();
        }
        _isPlaying = false;
    }
}
