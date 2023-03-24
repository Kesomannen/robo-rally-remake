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

    GameProperty _gameSettingsProperty;

    void OnEnable() {
        LobbySystem.LobbySettingsPropertyUpdated += UpdateSettingsProperty;
    }
    
    void OnDisable() {
        LobbySystem.LobbySettingsPropertyUpdated -= UpdateSettingsProperty;
    }
    
    void UpdateSettingsProperty(GameProperty settingsProperty) {
        if (settingsProperty != _gameSettingsProperty) return;

        if (settingsProperty.CanToggle) {
            _toggle.isOn = settingsProperty.Enabled;
        }

        var sliderActive = settingsProperty.HasValue && settingsProperty.Enabled;
        _slider.gameObject.SetActive(sliderActive);
        if (!sliderActive) return;

        _slider.value = settingsProperty.Value;
        var intValue = Mathf.RoundToInt(_slider.value);
        _text.text = $"{_name}: {intValue}{_valueAffix}";
    }

    public GameProperty GameProperty {
        set {
            if (_gameSettingsProperty != null) {
                _slider.onValueChanged.RemoveListener(RegisterValue);
                _toggle.onValueChanged.RemoveListener(RegisterToggle);
            }
            
            _gameSettingsProperty = value;
            
            _slider.minValue = value.Min;
            _slider.maxValue = value.Max;
            _slider.value = value.Value;
            _toggle.isOn = value.Enabled;

            var isServer = NetworkManager.Singleton.IsServer;
            _slider.interactable = isServer;
            _toggle.interactable = isServer;
            
            _slider.gameObject.SetActive(_gameSettingsProperty.HasValue);
            _toggle.gameObject.SetActive(_gameSettingsProperty.CanToggle);

            RegisterValue(_gameSettingsProperty.Value);
            RegisterToggle(_gameSettingsProperty.Enabled);
            
            if (!isServer) return;
            _slider.onValueChanged.AddListener(RegisterValue);
            _toggle.onValueChanged.AddListener(RegisterToggle);
        }
    }
    
    void RegisterToggle(bool value) {
        _gameSettingsProperty.Enabled = value;
        _slider.interactable = value;
        if (!value) {
            _text.text = _name;
        }
        LobbySystem.Instance.RefreshLobbyProperty(_gameSettingsProperty);
    }

    void RegisterValue(float value) {
        var intValue = Mathf.RoundToInt(value);
        _text.text = $"{_name}: {intValue}{_valueAffix}";
        
        if (intValue == _gameSettingsProperty.Value) return;
        _gameSettingsProperty.Value = (byte) intValue;
        LobbySystem.Instance.RefreshLobbyProperty(_gameSettingsProperty);
    }
}