using UnityEngine;

public class UIFocusManager : Singleton<UIFocusManager> {
    [SerializeField] RectTransform _mapParent, _uiParent;

    Focus _currentFocus;

    public Focus CurrentFocus {
        get => _currentFocus;
        set {
            if (value == _currentFocus) return;
            _currentFocus = value;
        }
    }

    public enum Focus {
        Balanced,
        Map,
        PlayerUI,
    }
}