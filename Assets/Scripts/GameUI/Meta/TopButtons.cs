﻿using UnityEngine;
using UnityEngine.InputSystem;

public class TopButtons : MonoBehaviour {
    [SerializeField] GameObject _log, _chat;
    [SerializeField] GameObject _chatNotification;
    [SerializeField] OverlayData<SettingsOverlay> _settingsOverlay;
    [SerializeField] SoundEffect _chatNotificationSound;
    [SerializeField] InputAction _toggleSettings;

    void Awake() {
        _chatNotification.SetActive(false);
    }

    void OnEnable() {
        Chat.OnMessageSent += OnNewChat;
        PhaseSystem.Current.OnValueChanged += OnPhaseChanged;
        
        _toggleSettings.Enable();
        _toggleSettings.performed += OnToggleSettingsPerformed;
    }
    
    void OnDisable() {
        Chat.OnMessageSent -= OnNewChat;
        PhaseSystem.Current.OnValueChanged -= OnPhaseChanged;
        
        _toggleSettings.Disable();
        _toggleSettings.performed -= OnToggleSettingsPerformed;
    }
    
    void OnToggleSettingsPerformed(InputAction.CallbackContext _) => ToggleSettings();

    void OnPhaseChanged(Phase prev, Phase next) {
        _log.SetActive(false);
        _chat.SetActive(false);
    }
    
    void OnNewChat(string str) {
        if (_chatNotification.activeSelf || _chat.activeSelf) return;
        _chatNotification.SetActive(true);
        _chatNotificationSound.Play();
    }

    public void ToggleSettings() {
        if (OverlaySystem.Instance.IsOverlayActive) return;
        
        OverlaySystem.Instance.PushAndShowOverlay(_settingsOverlay);
        _log.SetActive(false);
        _chat.SetActive(false);
    }

    public void ToggleChat() {
        if (OverlaySystem.Instance.IsOverlayActive) return;
        
        _log.SetActive(false);
        _chat.SetActive(!_chat.activeSelf);
        _chatNotification.SetActive(false);
    }

    public void ToggleLog() {
        if (OverlaySystem.Instance.IsOverlayActive) return;
        
        _log.SetActive(!_log.activeSelf);
        _chat.SetActive(false);
    }
}