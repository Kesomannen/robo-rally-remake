using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : Singleton<Scheduler> {
    static Queue<ScheduleItem> _routines = new();
    static bool _isPlaying;

    public static void AddItem(ScheduleItem item) {
        _routines.Enqueue(item);
        if (!_isPlaying) {
            instance.StartCoroutine(instance.PlayItems());
        }
    }

    public static void AddItem(IEnumerator routine) {
        _routines.Enqueue(new(routine));
        if (!_isPlaying) {
            instance.StartCoroutine(instance.PlayItems());
        }
    }

    public static void StartRoutine(IEnumerator routine) {
        instance.StartCoroutine(routine);
    }

    IEnumerator PlayItems() {
        _isPlaying = true;
        while (_routines.Count > 0) {
            var item = _routines.Dequeue();
            yield return new WaitForSeconds(item.Delay);
            foreach (var routine in item.Routines) {
                StartCoroutine(routine);
            }
        }
        _isPlaying = false;
    }
}

public struct ScheduleItem {
    public IEnumerator[] Routines;
    public float Delay;

    const float defaultDelay = 1f;

    public ScheduleItem(IEnumerator[] routines, float delay = defaultDelay) {
        Routines = routines;
        Delay = delay;
    }

    public ScheduleItem(IEnumerator routine, float delay = defaultDelay) {
        Routines = new IEnumerator[1] { routine };
        Delay = delay;
    }
}