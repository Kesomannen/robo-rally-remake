using UnityEngine;
using UnityEngine.InputSystem;

public class TopButtons : MonoBehaviour {
    [SerializeField] GameObject _log, _chat;
    [SerializeField] GameObject _shopButton, _chatButton;
    [SerializeField] GameObject _chatNotification;
    [SerializeField] Container<UpgradeCardData> _upgradePrefab;
    [SerializeField] OverlayData<SettingsOverlay> _settingsOverlay;
    [SerializeField] OverlayData<CollectionOverlay> _shopOverlay;
    [SerializeField] SoundEffect _chatNotificationSound;
    [SerializeField] InputAction _toggleSettings;

    void Awake() {
        _chatNotification.SetActive(false);
    }

    void Start() {
        _shopButton.SetActive(GameSystem.Settings.EnergyEnabled);
        _chatButton.SetActive(PlayerSystem.Players.Count > 1);
    }

    void OnEnable() {
        Chat.MessageSent += OnNewChat;
        GameSystem.CurrentPhase.ValueChanged += OnPhaseChanged;
        
        _toggleSettings.Enable();
        _toggleSettings.performed += OnToggleSettingsPerformed;
    }
    
    void OnDisable() {
        Chat.MessageSent -= OnNewChat;
        GameSystem.CurrentPhase.ValueChanged -= OnPhaseChanged;
        
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

    public void ToggleShop() {
        if (OverlaySystem.Instance.IsOverlayActive) return;
        OverlaySystem.Instance.PushAndShowOverlay(_shopOverlay).Init(_upgradePrefab, ShopPhase.Instance.ShopCards);
    }
}