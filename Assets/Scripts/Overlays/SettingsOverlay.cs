using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsOverlay : Overlay {
    [SerializeField] Slider _musicSlider, _sfxSlider;

    void Start() {
        _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    protected override void OnEnable() {
        base.OnEnable();
        _musicSlider.value = AudioSystem.Instance.MusicVolume;
        _sfxSlider.value = AudioSystem.Instance.SfxVolume;
    }

    static void OnSfxVolumeChanged(float value) {
        AudioSystem.Instance.SfxVolume = value;
    }

    static void OnMusicVolumeChanged(float value) {
        AudioSystem.Instance.MusicVolume = value;
    }

    public void LeaveGame() {
        NetworkSystem.ReturnToLobby();
    }
}