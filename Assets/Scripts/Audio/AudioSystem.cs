using System;
using UnityEngine;

public class AudioSystem : Singleton<AudioSystem> {
    [SerializeField] AudioChannel _musicChannel, _uiChannel, _sfxChannel;

    public static float MusicVolume { get; set; } = 0.5f;
    public static float SfxVolume { get; set; } = 0.5f;

    int _uiPriority;
    int _sfxPriority;

    const string MusicVolumePrefKey = "MusicVolume";
    const string SfxVolumePrefKey = "SfxVolume";

    protected override void Awake() {
        base.Awake();
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumePrefKey, MusicVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumePrefKey, SfxVolume);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        PlayerPrefs.SetFloat(MusicVolumePrefKey, MusicVolume);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, SfxVolume);
    }

    public void Play(SoundEffect sound, AudioTrack audioTrack) {
        var channel = GetChannel(audioTrack);
        var currentPriority = audioTrack == AudioTrack.UI ? _uiPriority : _sfxPriority;
        if (channel.Source.isPlaying && currentPriority > sound.Priority) {
            return;
        }
        
        channel.Source.pitch = sound.Pitch;
        channel.Source.volume = sound.Volume * (audioTrack == AudioTrack.Music ? MusicVolume : SfxVolume);
        channel.Source.clip = sound.Clip;
        channel.Source.Play();

        if (audioTrack == AudioTrack.UI) {
            _uiPriority = sound.Priority;
        } else if (audioTrack == AudioTrack.Sfx) {
            _sfxPriority = sound.Priority;
        }
    }

    AudioChannel GetChannel(AudioTrack audioTrack) {
        return audioTrack switch {
            AudioTrack.Music => _musicChannel,
            AudioTrack.UI => _uiChannel,
            AudioTrack.Sfx => _sfxChannel,
            _ => throw new System.NotImplementedException()
        };
    }
}

public enum AudioTrack {
    Music,
    UI,
    Sfx,
}

public static class SoundEffectExtensions {
    public static void Play(this SoundEffect sound) {
        AudioSystem.Instance.Play(sound, sound.PreferredAudioTrack);
    }
    
    public static void Play(this SoundEffect sound, AudioTrack audioTrack) {
        AudioSystem.Instance.Play(sound, audioTrack);
    }
}