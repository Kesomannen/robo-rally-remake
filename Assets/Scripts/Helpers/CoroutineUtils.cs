using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CoroutineUtils {
    static readonly Dictionary<float, WaitForSeconds> _cachedDelayObjects = new();
    public static WaitForSeconds Wait(float seconds) {
        return _cachedDelayObjects.EnforceKey(seconds, () => new WaitForSeconds(seconds));
    }
    
    public static IEnumerator WaitRoutine(float seconds) {
        yield return Wait(seconds);
    }

    static readonly WaitForEndOfFrame _endOfFrame = new();
    public static WaitForEndOfFrame WaitEndOfFrame() => _endOfFrame;

    public static IEnumerator RunRoutines(this MonoBehaviour owner, params IEnumerator[] routines) {
        var handles = routines.Select(r => new CoroutineHandle(owner, r)).ToArray();
        yield return new WaitWhile(() => handles.Any(h => !h.IsDone));
    }
    
    public static IEnumerator StaggerRoutines(params IEnumerator[] routines) {
        return routines.GetEnumerator();
    }
}