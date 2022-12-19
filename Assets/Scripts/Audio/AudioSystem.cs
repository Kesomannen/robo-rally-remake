using UnityEngine;

public class AudioSystem : Singleton<AudioSystem> {
    [SerializeField] AudioChannel _musicChannel, _uiChannel, _sfxChannel;

    public void Play(SoundEffect sound, AudioTrack audioTrack) {
        sound.Play(GetChannel(audioTrack).Source);
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