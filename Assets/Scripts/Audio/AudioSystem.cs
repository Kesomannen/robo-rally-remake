using System;
using System.Collections;
using UnityEngine;

public class AudioSystem : Singleton<AudioSystem> {
    [SerializeField] AudioChannel _musicChannel, _uiChannel, _sfxChannel;

    public static float MusicVolume {
        get => _musicVolume;
        set {
            var clampedValue = Mathf.Clamp(value, 0.01f, 1);
            
            var current = Instance._musicChannel.Source.volume;
            var currentBase = current / _musicVolume;
            Instance._musicChannel.Source.volume = currentBase * clampedValue;
            
            _musicVolume = clampedValue;
        }
    }
    public static float SfxVolume {
        get => _sfxVolume;
        set {
            var clampedValue = Mathf.Clamp(value, 0.01f, 1);
            
            var current = Instance._sfxChannel.Source.volume;
            var currentBase = current / _sfxVolume;
            Instance._sfxChannel.Source.volume = currentBase * clampedValue;
            
            current = Instance._uiChannel.Source.volume;
            currentBase = current / _sfxVolume;
            Instance._uiChannel.Source.volume = currentBase * clampedValue;
            
            _sfxVolume = clampedValue;
        }
    }

    int _uiPriority;
    int _sfxPriority;
    static float _musicVolume = 0.5f;
    static float _sfxVolume = 0.5f;

    const string MusicVolumePrefKey = "MusicVolume";
    const string SfxVolumePrefKey = "SfxVolume";

    protected override void Awake() {
        base.Awake();
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumePrefKey, _musicVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumePrefKey, _sfxVolume);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        PlayerPrefs.SetFloat(MusicVolumePrefKey, MusicVolume);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, SfxVolume);
    }

    public IEnumerator PlayAndWait(SoundEffect sound, AudioTrack audioTrack) {
        var channel = GetChannel(audioTrack);
        var currentPriority = audioTrack == AudioTrack.UI ? _uiPriority : _sfxPriority;
        if (channel.Source.isPlaying && currentPriority > sound.Priority) {
            yield break;
        }
        
        var pitch = sound.Pitch;
        var clip = sound.Clip;
        
        channel.Source.pitch = pitch;
        channel.Source.volume = sound.Volume * (audioTrack == AudioTrack.Music ? MusicVolume : SfxVolume);
        channel.Source.clip = clip;
        channel.Source.Play();

        if (audioTrack == AudioTrack.UI) {
            _uiPriority = sound.Priority;
        } else if (audioTrack == AudioTrack.Sfx) {
            _sfxPriority = sound.Priority;
        }

        yield return CoroutineUtils.Wait(clip.length / pitch);
    }
    
    public void Play(SoundEffect sound, AudioTrack audioTrack) {
        StartCoroutine(PlayAndWait(sound, audioTrack));
    }

    AudioChannel GetChannel(AudioTrack audioTrack) {
        return audioTrack switch {
            AudioTrack.Music => _musicChannel,
            AudioTrack.UI => _uiChannel,
            AudioTrack.Sfx => _sfxChannel,
            _ => throw new ArgumentOutOfRangeException()
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