using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsOverlay : Overlay {
    [SerializeField] Slider _musicSlider, _sfxSlider;
    [SerializeField] Optional<TMP_Text> _nameText;

    void Start() {
        _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    protected override void OnEnable() {
        base.OnEnable();
        _musicSlider.value = AudioSystem.MusicVolume;
        _sfxSlider.value = AudioSystem.SfxVolume;
        
        if (!_nameText.Enabled) return;
        _nameText.Value.text = PlayerSystem.HasInstance
            ? PlayerSystem.LocalPlayer.ToString()
            : LobbySystem.PlayerName;

        LobbySystem.NameChanged += OnNameChanged;
    }

    protected override void OnDisable() {
        base.OnDisable();
        LobbySystem.NameChanged -= OnNameChanged;
    }

    static void OnSfxVolumeChanged(float value) {
        AudioSystem.SfxVolume = value;
    }

    static void OnMusicVolumeChanged(float value) {
        AudioSystem.MusicVolume = value;
    }

    public void LeaveGame() {
        StartCoroutine(NetworkSystem.Instance.ReturnToLobby());
    }

    public void ChangeName() {
        LobbySystem.Instance.GatherName();
    }
    
    void OnNameChanged(string newName) {
        _nameText.Value.text = newName;
    }
}