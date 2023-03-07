using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsButton : MonoBehaviour {
    [SerializeField] OverlayData<SettingsOverlay> _overlay;
    [SerializeField] InputAction _toggleSettings;

    void OnEnable() {
        _toggleSettings.Enable();
        _toggleSettings.performed += OnToggleSettingsPerformed;
    }
    
    void OnDisable() {
        _toggleSettings.Disable();
        _toggleSettings.performed -= OnToggleSettingsPerformed;
    }
    
    void OnToggleSettingsPerformed(InputAction.CallbackContext context) => ToggleSettings();

    public void ToggleSettings() {
        if (OverlaySystem.Instance.IsOverlayActive) return;
        
        OverlaySystem.Instance.PushAndShowOverlay(_overlay);
    }
}