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
        LobbySystem.LobbySettingsPropertyUpdated += UpdateSettingsProperty;
    }
    
    void OnDisable() {
        LobbySystem.LobbySettingsPropertyUpdated -= UpdateSettingsProperty;
    }
    
    void UpdateSettingsProperty(LobbyProperty property) {
        if (property != _lobbyProperty) return;

        if (property.CanToggle) {
            _toggle.isOn = property.Enabled;
        }

        var sliderActive = property.HasValue && property.Enabled;
        _slider.gameObject.SetActive(sliderActive);
        if (!sliderActive) return;

        _slider.value = property.Value;
        var intValue = Mathf.RoundToInt(_slider.value);
        _text.text = $"{_name}: {intValue}{_valueAffix}";
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
        LobbySystem.Instance.RefreshLobbyProperty(_lobbyProperty);
    }

    void RegisterValue(float value) {
        var intValue = Mathf.RoundToInt(value);
        _text.text = $"{_name}: {intValue}{_valueAffix}";
        
        if (intValue == _lobbyProperty.Value) return;
        _lobbyProperty.Value = (byte) intValue;
        LobbySystem.Instance.RefreshLobbyProperty(_lobbyProperty);
    }
}