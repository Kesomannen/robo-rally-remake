using System.Collections.Generic;
using UnityEngine;

public static class Helpers {
    static readonly Dictionary<float, WaitForSeconds> _cachedDelayObjects = new();

    public static WaitForSeconds Wait(float seconds) {
        return _cachedDelayObjects.EnforceKey(seconds, () => new WaitForSeconds(seconds));
    }

    static readonly WaitForEndOfFrame _endOfFrame = new();

    public static WaitForEndOfFrame WaitEndOfFrame() => _endOfFrame;
}