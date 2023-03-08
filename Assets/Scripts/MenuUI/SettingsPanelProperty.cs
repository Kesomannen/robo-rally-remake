using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelProperty : MonoBehaviour {
    [SerializeField] string _name;
    [SerializeField] string _valueAffix;
    [SerializeField] Slider _slider;
    [SerializeField] Toggle _toggle;
    [SerializeField] TMP_Text _text;

    LobbyProperty _lobbyProperty;

    void OnEnable() {
        LobbySystem.OnLobbySettingsUpdated += UpdateSettings;
    }
    
    void OnDisable() {
        LobbySystem.OnLobbySettingsUpdated -= UpdateSettings;
    }
    
    void UpdateSettings(LobbySettings settings) {
        if (_lobbyProperty == null) return;
        
        if (_lobbyProperty.CanToggle) {
            _toggle.isOn = _lobbyProperty.Enabled;
        }
        if (_lobbyProperty.HasValue) {
            _slider.value = _lobbyProperty.Value;
            var intValue = Mathf.RoundToInt(_slider.value);
            _text.text = $"{_name}: {intValue}{_valueAffix}";
        }
    }

    public LobbyProperty LobbyProperty {
        set {
            if (_lobbyProperty != null) {
                _slider.onValueChanged.RemoveListener(RegisterValue);
                _toggle.onValueChanged.RemoveListener(RegisterToggle);
            }
            
            _lobbyProperty = value;
            _slider.minValue = value.Min;
            _slider.maxValue = value.Max;
            _slider.value = value.Value;
            _toggle.isOn = value.Enabled;

            var isServer = NetworkManager.Singleton.IsServer;
            _slider.interactable = isServer;
            _toggle.interactable = isServer;
            
            _slider.gameObject.SetActive(_lobbyProperty.HasValue);
            _toggle.gameObject.SetActive(_lobbyProperty.CanToggle);

            RegisterValue(_lobbyProperty.Value);
            RegisterToggle(_lobbyProperty.Enabled);
            
            if (!isServer) return;
            _slider.onValueChanged.AddListener(RegisterValue);
            _toggle.onValueChanged.AddListener(RegisterToggle);
        }
    }
    
    void RegisterToggle(bool value) {
        _lobbyProperty.Enabled = value;
        _slider.interactable = value;
        if (!value) {
            _text.text = _name;
        }
        LobbySystem.Instance.RefreshLobbySettings();
    }

    void RegisterValue(float value) {
        var intValue = Mathf.RoundToInt(value);
        _text.text = $"{_name}: {intValue}{_valueAffix}";
        
        if (intValue == _lobbyProperty.Value) return;
        _lobbyProperty.Value = (byte) intValue;
        LobbySystem.Instance.RefreshLobbySettings();
    }
}