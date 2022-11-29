using UnityEngine;
using UnityEngine.UI;

public static class CanvasUtils {
    static CanvasScaler _canvasScaler;
    static CanvasScaler CanvasScaler {
        get {
            if (_canvasScaler == null) {
                _canvasScaler = Object.FindObjectOfType<CanvasScaler>();
            }
            return _canvasScaler;
        }
    }

    public static Vector2 CanvasScale => CanvasScaler.transform.localScale;
    public static Vector2 ScreenScale => new Vector2(Screen.width, Screen.height) / CanvasScaler.referenceResolution;
}