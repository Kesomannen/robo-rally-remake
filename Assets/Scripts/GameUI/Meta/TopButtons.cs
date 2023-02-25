using UnityEngine;

public class TopButtons : MonoBehaviour {
    [SerializeField] GameObject _log, _chat;
    [SerializeField] GameObject _chatNotification;
    [SerializeField] OverlayData<SettingsOverlay> _settingsOverlay;
    [SerializeField] SoundEffect _chatNotificationSound;

    void Awake() {
        _chatNotification.SetActive(false);
    }

    void OnEnable() {
        Chat.OnMessageSent += OnNewChat;
    }
    
    void OnDisable() {
        Chat.OnMessageSent -= OnNewChat;
    }
    
    void OnNewChat(string str) {
        if (_chatNotification.activeSelf || _chat.activeSelf) return;
        _chatNotification.SetActive(true);
        _chatNotificationSound.Play();
    }

    public void ToggleSettings() {
        OverlaySystem.Instance.PushAndShowOverlay(_settingsOverlay);
        _log.SetActive(false);
        _chat.SetActive(false);
    }

    public void ToggleChat() {
        _log.SetActive(false);
        _chat.SetActive(!_chat.activeSelf);
        _chatNotification.SetActive(false);
    }

    public void ToggleLog() {
        _log.SetActive(!_log.activeSelf);
        _chat.SetActive(false);
    }
}