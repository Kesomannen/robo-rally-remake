using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : Singleton<Scheduler> {
    static Stack<IScheduleItem> _itemStack = new();
    static bool _isPlaying;

    public const float DefaultDelay = 0.5f;
    public const float DefaultInterval = 0.25f;

    public static void AddItem(IScheduleItem item) {
        _itemStack.Push(item);
        if (!_isPlaying) {
            Instance.StartCoroutine(Instance.PlayItems());
        }
    }

    public static Coroutine StartRoutine(IEnumerator routine) {
        return Instance.StartCoroutine(routine);
    }

    public static IEnumerator WaitUntilQueueEmpty() {
        yield return new WaitUntil(() => _itemStack.Count == 0);
    }

    IEnumerator PlayItems() {
        _isPlaying = true;
        while (_itemStack.Count > 0) {
            yield return _itemStack.Pop().Play();
        }
        _isPlaying = false;
    }
}
