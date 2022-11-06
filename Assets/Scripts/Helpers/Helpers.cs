using System.Collections.Generic;
using UnityEngine;

public static class Helpers {
    static readonly Dictionary<float, WaitForSeconds> _cachedDelayObjects = new();

    public static WaitForSeconds Wait(float seconds) {
        if (!_cachedDelayObjects.ContainsKey(seconds)) {
            _cachedDelayObjects.Add(seconds, new WaitForSeconds(seconds));
        }
        return _cachedDelayObjects[seconds];
    }
}