using UnityEngine;
using UnityEngine.UI;

public static class CanvasUtils {
    static CanvasScaler _canvasScaler;
    public static CanvasScaler CanvasScaler {
        get {
            if (_canvasScaler == null) {
                _canvasScaler = Object.FindObjectOfType<CanvasScaler>();
            }
            return _canvasScaler;
        }
    }

    public static Vector2 Scale => CanvasScaler.transform.localScale;
}