using UnityEngine;

public class TopButtons : MonoBehaviour {
    [SerializeField] GameObject _log;
    [SerializeField] OverlayData<SettingsOverlay> _settingsOverlay;

    public void Settings() {
        OverlaySystem.Instance.PushAndShowOverlay(_settingsOverlay);
    }

    public void Chat() {
        
    }

    public void Log() {
        _log.SetActive(!_log.activeSelf);
    }
}